using UnityEngine;
using TMPro;
using System.Collections;

public class CodePanelManager : MonoBehaviour
{
    public static CodePanelManager Instance;

    [Header("UI")]
    public GameObject codePanel;
    public TMP_Text codeDisplay;
    public TMP_Text errorText;

    [Header("Settings")]
    public string correctCode = "1234"; // đổi mã ở đây

    private string inputCode = "";
    public bool isActive = false;
    private System.Action onCorrect;

    void Awake() => Instance = this;

    void Update()
    {
        if (!isActive) return;

        // Nhận input số
        foreach (char c in Input.inputString)
        {
            if (char.IsDigit(c) && inputCode.Length < 4)
            {
                inputCode += c;
                UpdateDisplay();
            }
        }

        // Backspace xóa ký tự cuối
        if (Input.GetKeyDown(KeyCode.Backspace) && inputCode.Length > 0)
        {
            inputCode = inputCode.Substring(0, inputCode.Length - 1);
            UpdateDisplay();
        }

        // Enter xác nhận
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            CheckCode();

        // Escape đóng
        if (Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();
    }

    public void OpenPanel(System.Action onCorrectCallback)
    {
        inputCode = "";
        isActive = true;
        onCorrect = onCorrectCallback;
        codePanel.SetActive(true);
        errorText.gameObject.SetActive(false);
        UpdateDisplay();
        FindObjectOfType<PlayerController>().StopMovement();
    }

    void CheckCode()
    {
        if (inputCode == correctCode)
        {
            ClosePanel();
            onCorrect?.Invoke();
        }
        else
        {
            inputCode = "";
            UpdateDisplay();
            StartCoroutine(ShowError());
        }
    }

    void UpdateDisplay()
    {
        string display = "";
        for (int i = 0; i < 4; i++)
            display += (i < inputCode.Length) ? inputCode[i] + " " : "_ ";
        codeDisplay.text = display.Trim();
    }

    IEnumerator ShowError()
    {
        errorText.gameObject.SetActive(true);
        errorText.text = "Không phải... phải đi tìm mã...";
        yield return new WaitForSeconds(1.5f);
        errorText.gameObject.SetActive(false);
    }

    void ClosePanel()
    {
        codePanel.SetActive(false);
        isActive = false;
        FindObjectOfType<PlayerController>().ResumeMovement();
    }
}