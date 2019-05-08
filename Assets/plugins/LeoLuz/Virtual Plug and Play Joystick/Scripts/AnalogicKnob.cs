using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LeoLuz.PropertyAttributes;

namespace LeoLuz.PlugAndPlayJoystick
{
    public class AnalogicKnob : ButtonBase
    {
        [InputAxesListDropdown]
        public string HorizontalAxis = "Horizontal";
        [InputAxesListDropdown]
        public string VerticalAxis = "Vertical";
        public enum ClampMode { Box, Circle }
        public ClampMode clampMode;
        [Tooltip("Maximum distance at which the knob can be turned, in units of RectTransform position.")]
        public float TurnLimit = 107f;
        private float _MaxDistance;
        [Tooltip("Speed that the knob returns.")]
        public float ReturnSpeed = 10f;
        public bool autoHide;
        [hideIf("autoHide", false)]
        public float fadeInDuration = 0.5f;
        [hideIf("autoHide", false)]
        public float fadeOutDuration = 2f;
        [hideIf("autoHide", false)]
        public float StartFadeOutDuration = 5f;
        [hideIf("autoHide", false)]
        public Image KnobBackground;
        [hideIf("autoHide", false)]
        public bool knobBackgroundFollowKnob;
        [HideInInspector]
        public Bounds AnchoredAreaBounds;
        [HideInInspector]
        public Bounds NormalizedAnchoredAreaBounds;
        [HideInInspector]
        public Vector2 MinScreenAreaBounds;
        [HideInInspector]
        public Vector2 MaxScreenAreaBounds;
        [HideInInspector]
        public Vector2 ScreenCenterAreaBounds;
        [HideInInspector]
        public float ScreenRadiusAreaBounds;
        private RectTransform rectTransform;
        private Image imageUI;
        private Vector2 StartPosition;
        private Vector2 CurrentKnobPosition;
        private Vector2 RawAxis;
        [ReadOnlyInPlayMode]
        public Vector2 NormalizedAxis;
        private Vector2 ScreenPixels;
        private Vector2 CanvasSize;
        private Vector3 canvasInitialPoint;
        private float CanvasScale;
        private Vector2 ScreenToAnchorPositionConversionConstant;
        private float ScreenUnitsToWorldUnitsConversionConstant;
        private Vector2 AnalogicStartPosition;
        private Touch AnalogTouch;
        private int InsideAreaTouchId = -1;
        private bool Released = false;
        Vector2 lastFrameNormalizedAxis;
        private delegate bool method(Vector2 position);
        private method CheckArea;
        private DeviceOrientation _devOrientation;

#if UNITY_EDITOR
        private bool OrderOfScriptChanged;

        public void OnDrawGizmosSelected()
        {

            if (!OrderOfScriptChanged)
            {
                // Get the name of the script we want to change it's execution order
                string scriptName = typeof(AnalogicKnob).Name;

                // Iterate through all scripts (Might be a better way to do this?)
                foreach (UnityEditor.MonoScript monoScript in UnityEditor.MonoImporter.GetAllRuntimeMonoScripts())
                {
                    // If found our script
                    if (monoScript.name == scriptName && UnityEditor.MonoImporter.GetExecutionOrder(monoScript) != -2000)
                    {
                        UnityEditor.MonoImporter.SetExecutionOrder(monoScript, -2000);
                    }
                }
                OrderOfScriptChanged = true;
            }
        }
#endif
        //private void OnGUI()
        //{
        //    GUILayout.BeginVertical();
        //    var lab = new GUIStyle();
        //    lab.fontStyle = FontStyle.Bold;
        //    lab.normal.textColor = Color.red;
        //    GUILayout.Label("Released: " + Released, lab);
        //    GUILayout.Label("AnalogicStartPosition: " + AnalogicStartPosition, lab);
        //    GUILayout.EndVertical();
        //}
        public override void Start()
        {
            base.Start();
            //  Input.RegisterAxisMobile(VerticalAxis);
            //  Input.RegisterAxisMobile(HorizontalAxis);

            rectTransform = GetComponent<RectTransform>();
            imageUI = GetComponent<Image>();
            if (rectTransform == null)
            {
                Debug.Log("Specify the object of the knob");
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("canvas not found, put this object as children of an canvas.");
            }

            RectTransform CanvasRect = canvas.GetComponent<RectTransform>();

            AnchoredAreaBounds = new Bounds(new Vector2(NormalizedAnchoredAreaBounds.center.x * CanvasRect.sizeDelta.x, NormalizedAnchoredAreaBounds.center.y * CanvasRect.sizeDelta.y)
                , new Vector2(NormalizedAnchoredAreaBounds.size.x * CanvasRect.sizeDelta.x, NormalizedAnchoredAreaBounds.size.y * CanvasRect.sizeDelta.y));


            AnalogicStartPosition = transform.position;
            ScreenPixels = new Vector2(Screen.width, Screen.height);

            CanvasSize = CanvasRect.sizeDelta;
            CanvasScale = canvas.transform.lossyScale.x;
            ScreenToAnchorPositionConversionConstant = new Vector2(CanvasSize.x / ScreenPixels.x, CanvasSize.y / ScreenPixels.y);
            ScreenUnitsToWorldUnitsConversionConstant = ScreenToAnchorPositionConversionConstant.y * CanvasScale;
            _MaxDistance = TurnLimit / ScreenToAnchorPositionConversionConstant.y;
            canvasInitialPoint = canvas.transform.position + (new Vector3(-CanvasSize.x * canvas.transform.lossyScale.x * 0.5f, -CanvasSize.y * canvas.transform.lossyScale.y * 0.5f));
            Vector3 knobUnAnchoredPositionOnCanvas = (transform.position - canvasInitialPoint) / canvas.transform.lossyScale.y;
            MinScreenAreaBounds = (knobUnAnchoredPositionOnCanvas + AnchoredAreaBounds.min) / ScreenToAnchorPositionConversionConstant.y;
            MaxScreenAreaBounds = (knobUnAnchoredPositionOnCanvas + AnchoredAreaBounds.max) / ScreenToAnchorPositionConversionConstant.y;
            ScreenCenterAreaBounds = (knobUnAnchoredPositionOnCanvas + AnchoredAreaBounds.center) / ScreenToAnchorPositionConversionConstant.y;
            ScreenRadiusAreaBounds = AnchoredAreaBounds.extents.x / ScreenToAnchorPositionConversionConstant.y;

            if (clampMode == ClampMode.Box)
                CheckArea = CheckBoxArea;
            else
                CheckArea = CheckCircleArea;

            Released = true;
            InsideAreaTouchId = -1;

            if (autoHide)
            {
                imageUI.CrossFadeAlpha(0f, 5f, true);
                if (KnobBackground != null)
                {
                    KnobBackground.CrossFadeAlpha(0f, 5f, true);
                }
            }

            _devOrientation = UnityEngine.Input.deviceOrientation;
        }



        void Update()
        {
            if (_devOrientation != UnityEngine.Input.deviceOrientation)
                Start();

            lastFrameNormalizedAxis = NormalizedAxis;

            //SIMULATED MOBILE VIRTUAL JOYSTICK KNOB ON EDITOR AND EFFETIVE MOBILE VIRTUAL JOYSTICK KNOB
            if (TouchCont > 0)
            {
                if (Released)
                {
                    InsideAreaTouchId = GetAnalogTouchIDInsideArea(); //-1 = none
                }

                if (InsideAreaTouchId != -1)
                {
                    AnalogTouch = GetTouch(InsideAreaTouchId);
                    if (Released)
                    {
                        if (AnalogTouch.phase == TouchPhase.Began)
                        {
                            Released = false;
                            TouchBegan(AnalogTouch.position);
                        }
                    }
                    else
                    {
                        if (AnalogTouch.phase == TouchPhase.Moved || AnalogTouch.phase == TouchPhase.Stationary)
                            TouchMove(AnalogTouch.position);
                        else if (AnalogTouch.phase == TouchPhase.Ended)
                            TouchEnd();
                    }
                }
                else
                {
                    Released = true;
                    //return;

                }
            }
            else
            {
                InsideAreaTouchId = -1;
                Released = true;
            }

            if (Released == true)
            {
                rectTransform.position = Vector2.Lerp(transform.position, AnalogicStartPosition, ReturnSpeed * Time.unscaledDeltaTime);
            }

            Input.SetAxisMobile(HorizontalAxis, NormalizedAxis.x);
            Input.SetAxisMobile(VerticalAxis, NormalizedAxis.y);

            if (Mathf.Abs(lastFrameNormalizedAxis.x) < 0.2f && NormalizedAxis.x != 0f)
            {
                Input.PressButtonDownMobile(HorizontalAxis);
            }
            if (Mathf.Abs(lastFrameNormalizedAxis.y) < 0.2f && NormalizedAxis.y != 0f)
            {
                Input.PressButtonDownMobile(VerticalAxis);
            }
        }

        bool CheckBoxArea(Vector2 pos)
        {
            if (pos.x > MinScreenAreaBounds.x && pos.x < MaxScreenAreaBounds.x && pos.y > MinScreenAreaBounds.y && pos.y < MaxScreenAreaBounds.y)
                return true;
            else
                return false;
        }

        bool CheckCircleArea(Vector2 pos)
        {
            if (Vector2.Distance(pos, ScreenCenterAreaBounds) < ScreenRadiusAreaBounds)
                return true;
            else
                return false;
        }



        void TouchBegan(Vector2 TouchPosition)
        {
            Released = false;
            StartPosition = TouchPosition;
            //  rectTransform.anchoredPosition = UnityEngine.Input.mousePosition * ScreenToAnchorPositionConversionConstant.y;
            transform.position = canvasInitialPoint + (Vector3)TouchPosition * ScreenUnitsToWorldUnitsConversionConstant;

            if (autoHide)
            {
                imageUI.CrossFadeAlpha(1f, fadeInDuration, true);
                if (KnobBackground != null)
                {
                    KnobBackground.CrossFadeAlpha(1f, fadeInDuration, true);
                    if (knobBackgroundFollowKnob)
                    {
                        KnobBackground.transform.position = transform.position;
                        AnalogicStartPosition = transform.position;
                    }
                }
            }
        }

        void TouchMove(Vector3 position)
        {
            CurrentKnobPosition = position;
            RawAxis = CurrentKnobPosition - StartPosition;
            var direction = RawAxis.normalized;
            var distance = RawAxis.magnitude;
            var normalizedDistance = Mathf.Clamp(distance / _MaxDistance, 0f, 1.05f);
            NormalizedAxis = direction * normalizedDistance;
            transform.position = canvasInitialPoint + (Vector3)((StartPosition + (NormalizedAxis * _MaxDistance)) * ScreenUnitsToWorldUnitsConversionConstant);
            //rectTransform.anchoredPosition = (StartPosition + (NormalizedAxis * _MaxDistance)) * ScreenToAnchorPositionConversionConstant.y;
        }

        void TouchEnd()
        {
            Released = true;
            InsideAreaTouchId = -1;
            NormalizedAxis = new Vector2(0f, 0f);

            if (autoHide)
            {
                imageUI.CrossFadeAlpha(0f, fadeOutDuration, true);
                if (KnobBackground != null)
                {
                    KnobBackground.CrossFadeAlpha(0f, fadeOutDuration, true);
                }
            }
        }

        int GetAnalogTouchIDInsideArea()
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL

            if ((UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonUp(0) || UnityEngine.Input.GetMouseButton(0)) && CheckArea(UnityEngine.Input.mousePosition))
                return 0;
            else
                return -1;
#endif
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1)
            for (int i = 0; i < Input.touchCount; i++)
            {
                if (CheckArea(Input.GetTouch(i).position))
                    return i;
            }
            return -1;
#endif
        }


        #region TOUCH ABSTRACTION
        public int TouchCont
        {
            get
            {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
                return (UnityEngine.Input.GetMouseButtonDown(0) || UnityEngine.Input.GetMouseButtonUp(0) || UnityEngine.Input.GetMouseButton(0)) ? 1 : 0;
#endif
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1)
                return Input.touchCount;
#endif

            }
        }

        Touch GetTouch(int id)
        {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
            var touch = new Touch();
            touch.position = UnityEngine.Input.mousePosition;
            touch.phase =
                UnityEngine.Input.GetMouseButtonDown(0) ? TouchPhase.Began :
                UnityEngine.Input.GetMouseButtonUp(0) ? TouchPhase.Ended :
                UnityEngine.Input.GetMouseButton(0) ? TouchPhase.Moved :
                TouchPhase.Canceled;

            return touch;

#endif
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS || UNITY_WP_8 || UNITY_WP_8_1)
            return Input.GetTouch(InsideAreaTouchId);
#endif
        }

        #endregion
    }
}
