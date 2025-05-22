using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SimpleJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Tooltip("Background Image do joystick")]
    public RectTransform background;
    [Tooltip("Imagem do pegador (handle)")]
    public RectTransform handle;

    [Range(0f, 2f)]
    public float handleRange = 1f;
    [Range(0f, 1f)]
    public float deadZone = 0f;

    Vector2 input = Vector2.zero;

    void Start()
    {
        // Se não tiver sido atribuído no Inspector
        if (background == null)
            background = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        // Converte a posição da tela para coordenadas locais do joystick
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out position
        );

        // Normaliza baseado no tamanho do background
        position.x = (position.x / background.sizeDelta.x);
        position.y = (position.y / background.sizeDelta.y);

        // Centraliza a origem em (0,0) e aplica dead zone
        input = new Vector2(position.x * 2, position.y * 2);
        if (input.magnitude < deadZone)
        {
            input = Vector2.zero;
        }
        else
        {
            input = input.normalized * ((input.magnitude - deadZone) / (1 - deadZone));
            input = Vector2.ClampMagnitude(input, 1f);
        }

        // Move o handle de acordo
        handle.anchoredPosition = new Vector2(
            input.x * (background.sizeDelta.x / 2) * handleRange,
            input.y * (background.sizeDelta.y / 2) * handleRange
        );
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        // Reset handle para o centro
        handle.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Valor horizontal normalizado de -1 a 1
    /// </summary>
    public float Horizontal => input.x;

    /// <summary>
    /// Valor vertical normalizado de -1 a 1
    /// </summary>
    public float Vertical => input.y;
}
