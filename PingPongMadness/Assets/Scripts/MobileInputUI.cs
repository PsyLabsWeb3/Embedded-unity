using UnityEngine;
using UnityEngine.UI;

public class MobileInputUI : MonoBehaviour
{
    public static MobileInputUI Instance;

    [HideInInspector] public bool IsMovingUp;
    [HideInInspector] public bool IsMovingDown;

    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        // upButton.onClick.AddListener(() => IsMovingUp = true);
        // downButton.onClick.AddListener(() => IsMovingDown = true);
    }

   public void StartMoveUp()
    {
        IsMovingUp = true;
        Debug.Log("⬆️ PRESIONADO");
    }

    public void StopMoveUp()
    {
        IsMovingUp = false;
        Debug.Log("⬆️ LIBERADO");
    }

    public void StartMoveDown()
    {
        IsMovingDown = true;
        Debug.Log("⬇️ PRESIONADO");
    }

    public void StopMoveDown()
    {
        IsMovingDown = false;
        Debug.Log("⬇️ LIBERADO");
    }
}
