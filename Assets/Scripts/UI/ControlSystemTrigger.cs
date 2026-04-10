using UnityEngine;
using UnityEngine.EventSystems;

namespace Gazze.UI
{
    /// <summary>
    /// UI elementlerinin (butonlar, joystick) PlayerController ile iletişim kurmasını sağlayan yardımcı bileşen.
    /// </summary>
    public class ControlSystemTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        public enum TriggerType
        {
            LeftButton,
            RightButton,
            GasPedal,
            BrakePedal,
            Joystick
        }

        public TriggerType type;
        
        [Header("Joystick Ayarları")]
        public RectTransform joystickBase;
        public float maxRadius = 100f;

        private PlayerController player;
        private Vector2 currentInput;

        private void Start()
        {
            player = PlayerController.Instance;
            if (player == null) player = FindFirstObjectByType<PlayerController>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateStatus(true, eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            UpdateStatus(false, eventData);
            if (type == TriggerType.Joystick)
            {
                currentInput = Vector2.zero;
                player.OnJoystickUpdate(Vector2.zero);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (type == TriggerType.Joystick && joystickBase != null)
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(joystickBase, eventData.position, eventData.pressEventCamera, out localPoint))
                {
                    currentInput = Vector2.ClampMagnitude(localPoint / maxRadius, 1f);
                    player.OnJoystickUpdate(currentInput);
                }
            }
        }

        private void UpdateStatus(bool isDown, PointerEventData eventData)
        {
            if (player == null) player = PlayerController.Instance;
            if (player == null) return;

            switch (type)
            {
                case TriggerType.LeftButton:
                    player.OnLeftButton(isDown);
                    break;
                case TriggerType.RightButton:
                    player.OnRightButton(isDown);
                    break;
                case TriggerType.GasPedal:
                    player.OnGasButton(isDown);
                    break;
                case TriggerType.BrakePedal:
                    player.OnBrakeButton(isDown);
                    break;
                case TriggerType.Joystick:
                    if (isDown) OnDrag(eventData);
                    break;
            }
        }
    }
}
