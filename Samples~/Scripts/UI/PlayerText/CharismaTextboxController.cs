using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CharismaSDK.PlugNPlay
{
    /// <summary>
    /// Collects all defined textbox locations and places UI for these
    /// The UI then tracks the position of the locations depending on the Main Camera object
    /// </summary>
    internal class CharismaTextboxController : MonoBehaviour
    {
        [ReadOnly]
        [SerializeField]
        private List<CharismaTextBoxLocation> _textboxLocations;
        [ReadOnly]
        [SerializeField]
        private List<CharismaTextBoxUI> _uiTextboxes;
        
        private RectTransform _canvas;
        private RectTransform _boundary;

        [SerializeField]
        private GameObject _textboxPrefab;

        private void Start()
        {
            _textboxLocations = new List<CharismaTextBoxLocation>();
            _uiTextboxes = new List<CharismaTextBoxUI>();

            _textboxLocations = GameObject.FindObjectsOfType<CharismaTextBoxLocation>().ToList();

            _canvas = this.transform.parent.GetComponent<RectTransform>();
            _boundary = this.gameObject.GetComponent<RectTransform>();

            // create a textbox for each valid TextboxLocationComponent
            foreach (var location in _textboxLocations)
            {
                var name = location.ActorName;
                var uiTextbox = GameObject.Instantiate(_textboxPrefab, this.gameObject.transform);
                uiTextbox.name = name + " Display";

                var textbox = uiTextbox.GetComponent<CharismaTextBoxUI>();
                location.AddUIComponent(textbox);
                _uiTextboxes.Add(textbox);
            }
        }

        private void LateUpdate()
        {
            if(_textboxLocations.Count == 0)
            {
                return;
            }

            UpdateTextboxInScreen();
            UpdateTextboxOverlap();
        }

        private void UpdateTextboxInScreen()
        {
            var count = _uiTextboxes.Count;

            for (int i = 0; i < count; i++)
            {
                var distance = Vector3.Distance(Camera.main.transform.position, _textboxLocations[i].transform.position);
                _textboxLocations[i].SetDistance(distance);
                _uiTextboxes[i].SetDistance(distance);

                var viewportPoint = Camera.main.WorldToScreenPoint(_textboxLocations[i].transform.position);

                // Only set up for Overlay canvas types
                // need to supply camera otherwise
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_boundary, viewportPoint, null, out var resultScreenPoint);

                Vector3 fitPoint;
                bool isOnScreen;

                // if viewport z is negative, its behind the camera not in front
                if (viewportPoint.z >= 0)
                {
                    fitPoint = FitViewportPointToCanvas(resultScreenPoint, _uiTextboxes[i].TextParent, out isOnScreen);
                }
                else
                {
                    fitPoint = SnapToClosestLimit(resultScreenPoint, _uiTextboxes[i].TextParent);
                    isOnScreen = false;
                }

                _uiTextboxes[i].SetIsOnScreen(isOnScreen);
                _uiTextboxes[i].TextParent.anchoredPosition = fitPoint;
            }
        }

        private Vector3 SnapToClosestLimit(Vector2 resultScreenPoint, RectTransform rect)
        {
            var snappedPosition = Vector3.zero;

            var rectOffsetX = rect.sizeDelta.x / 2 * rect.localScale.x;

            var minXlimit = -_canvas.sizeDelta.x / 2 + rectOffsetX;
            var maxXlimit = _canvas.sizeDelta.x / 2 - rectOffsetX;

            if(resultScreenPoint.x > 0)
            {
                snappedPosition.x = maxXlimit;
            }
            else
            {
                snappedPosition.x = minXlimit;
            }

            return snappedPosition;
        }

        private Vector3 FitViewportPointToCanvas(Vector3 viewportPoint, RectTransform rect, out bool isOnScreen)
        {
            isOnScreen = true;

           // need half size to know how to fit into the boundary
            var rectOffsetX = rect.sizeDelta.x / 2 * rect.localScale.x;
            var rectOffsetY = rect.sizeDelta.y / 2 * rect.localScale.y;

            var minXlimit = -_canvas.sizeDelta.x / 2 + rectOffsetX;
            var maxXlimit = _canvas.sizeDelta.x /2 - rectOffsetX;
            var minYLimit = -_canvas.sizeDelta.y / 2 + rectOffsetY;
            var maxYlimit = _canvas.sizeDelta.y / 2 - rectOffsetY;

            // apply limits
            var clampedX = Math.Clamp(viewportPoint.x, minXlimit, maxXlimit);
            var clampedY = Math.Clamp(viewportPoint.y, minYLimit, maxYlimit);

            if (clampedX != viewportPoint.x
                || clampedY != viewportPoint.y)
            {
                isOnScreen = false;
            }

            return new Vector3(clampedX, clampedY, 0);
        }

        private void UpdateTextboxOverlap()
        {
            var count = _uiTextboxes.Count;

            for (int i = 0; i < count - 1; i++)
            {
                if (!_uiTextboxes[i].TextParent.gameObject.activeSelf)
                {
                    continue;
                }

                var transformA = _uiTextboxes[i].TextParent;
                var rectA = transformA.rect;
                rectA.position = transformA.anchoredPosition;
                rectA.size *= transformA.localScale;

                for (int j = i + 1; j < count; j++)
                {
                    if (!_uiTextboxes[j].TextParent.gameObject.activeSelf)
                    {
                        continue;
                    }

                    var transformB = _uiTextboxes[j].TextParent;
                    var rectB = transformB.rect;
                    rectB.position = transformB.anchoredPosition;
                    rectB.size *= transformA.localScale;

                    if (rectA.Overlaps(rectB))
                    {
                        // determine whos closer to the center
                        var distanceToCenterA = Vector2.Distance(rectA.position, Vector2.zero);
                        var distanceToCenterB = Vector2.Distance(rectB.position, Vector2.zero);

                        Rect result;

                        // move whoever is closer to the center - less likely to get kicked out of bounds
                        if(distanceToCenterA > distanceToCenterB)
                        {
                            result = GetNonCollidingPositionForTargetRect(rectA, rectB, out var pushback, out var excess);
                            transformA.anchoredPosition = result.position;
                            transformB.anchoredPosition = transformB.anchoredPosition - excess + pushback;
                        }
                        else 
                        {
                            result = GetNonCollidingPositionForTargetRect(rectB, rectA, out var pushback, out var excess);
                            transformB.anchoredPosition = result.position;
                            transformA.anchoredPosition = transformA.anchoredPosition - excess + pushback;
                        }
                    }
                }
            }
        }

        private Rect GetNonCollidingPositionForTargetRect(Rect targetRect, Rect collidingRect, out Vector2 pushback, out Vector2 excess)
        {
            Rect result = targetRect;
            excess = Vector2.zero;
            pushback = Vector2.zero;

            Vector2 normal = collidingRect.position - targetRect.position;

            float minimumXDistance = targetRect.size.x / 2 + collidingRect.size.x / 2;
            float minimumYDistance = targetRect.size.y / 2 + collidingRect.size.y / 2;

            bool foundCollision = false;

            var overlapX = minimumXDistance - Math.Abs(normal.x);
            var overlapY = minimumYDistance - Math.Abs(normal.y);
            
            // account for overlapping from other direction
            // where the distance is much longer
            if (overlapX > 0)
            {
                foundCollision = true;
            }
            else if(overlapY > 0)
            {
                foundCollision = true;
            }

            if (foundCollision)
            {
                var xSign = Math.Sign(normal.x);
                if (xSign == 0)
                {
                    // 0 sign means a perfect X overlap - set to default value
                    xSign = 1;
                }

                var newPosition = result.position;
                if (overlapX > 0)
                {
                    var penetrationX = (overlapX) * xSign;
                    newPosition.x -= penetrationX / 2;
                    pushback.x += penetrationX / 2;
                }

                // re-assign the rect to re-initialise all the values needed in following function
                result = new Rect(newPosition, result.size);
                excess = GetExcessOutsideOfBounds(result);

                var clampedPosition = result.position;

                // clamp the position on screen
                clampedPosition.x = Mathf.Clamp(result.position.x,
                    -_boundary.rect.width / 2 + result.size.x / 2,
                    _boundary.rect.width / 2 - result.size.x / 2);

                clampedPosition.y = Mathf.Clamp(result.position.y, 
                    -_boundary.rect.height / 2 + result.size.y / 2,
                    _boundary.rect.height / 2 - result.size.y / 2);

                result.position = clampedPosition;
            }

            return result;
        }

        private Vector2 GetExcessOutsideOfBounds(Rect result)
        {
            Vector2 excess = Vector2.zero;

            var resultXmax = result.position.x + result.size.x / 2;
            var resultXmin = result.position.x - result.size.x / 2;
            var resultYmax = result.position.y + result.size.y / 2;
            var resultYmin = result.position.y - result.size.y / 2;

            var xmax = _boundary.rect.width / 2;
            var xmin = -_boundary.rect.width / 2;
            var ymax = _boundary.rect.height / 2;
            var ymin = -_boundary.rect.height / 2;

            if (resultXmax > xmax)
            {
                excess.x = resultXmax - xmax;
            }

            if (resultXmin < xmin)
            {
                excess.x = resultXmin - xmin;
            }

            if (resultYmax > ymax)
            {
                excess.y = resultYmax - ymax;
            }

            if (resultYmin < ymin)
            {
                excess.y =resultYmin - ymin;
            }

            return excess;
        }
    }
}