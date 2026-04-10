using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// UI butonlarına (Gaz/Fren vb.) basılma ve bırakılma olaylarını kontrol ederek PlayerController'a iletir.
/// </summary>
public class UIButtonHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private void Start()
    {
        // Debug.Log($"UIButtonHandler: {gameObject.name} başlatıldı. ActionType: {actionType}");
        
        // Objenin Raycast Target ayarını kontrol et (Sadece bilgilendirme için)
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null && !img.raycastTarget)
        {
            Debug.LogWarning($"UIButtonHandler: {gameObject.name} objesinin Image bileşeninde 'Raycast Target' kapalı! Tıklamalar çalışmayabilir.");
        }
    }

    /// <summary> Butonun gerçekleştirebileceği eylem tipleri. </summary>
    public enum ActionType { Gas, Brake, Boost, Left, Right }
    
    [Header("Buton Eylemi")]
    /// <summary> Bu butonun gerçekleştireceği eylem tipi. </summary>
    [Tooltip("Bu butonun gerçekleştireceği eylem tipi.")]
    public ActionType actionType;

    /// <summary>
    /// Butona basıldığında tetiklenen Unity EventSystems metodu.
    /// </summary>
    /// <param name="eventData">İşaretçi (fare/dokunmatik) verisi.</param>
    public void OnPointerDown(PointerEventData eventData)
    {
        if (PlayerController.Instance == null)
        {
            Debug.LogError("UIButtonHandler: PlayerController.Instance bulunamadı!");
            return;
        }
        
        // Eylem tipine göre PlayerController'daki ilgili basılma metodunu çağır
        switch (actionType)
        {
            case ActionType.Gas: PlayerController.Instance.GasDown(); break;
            case ActionType.Brake: PlayerController.Instance.BrakeDown(); break;
            case ActionType.Boost: PlayerController.Instance.BoostDown(); break;
            case ActionType.Left: PlayerController.Instance.MoveLeftDown(); break;
            case ActionType.Right: PlayerController.Instance.MoveRightDown(); break;
        }
    }

    /// <summary>
    /// Buton bırakıldığında tetiklenen Unity EventSystems metodu.
    /// </summary>
    /// <param name="eventData">İşaretçi (fare/dokunmatik) verisi.</param>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (PlayerController.Instance == null) return;

        // Eylem tipine göre PlayerController'daki ilgili bırakılma metodunu çağır
        switch (actionType)
        {
            case ActionType.Gas: PlayerController.Instance.GasUp(); break;
            case ActionType.Brake: PlayerController.Instance.BrakeUp(); break;
            case ActionType.Boost: PlayerController.Instance.BoostUp(); break;
            case ActionType.Left: 
            case ActionType.Right: 
                PlayerController.Instance.StopHorizontal(); 
                break;
        }
    }
}
