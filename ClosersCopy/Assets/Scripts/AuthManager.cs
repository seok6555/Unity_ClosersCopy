using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

/// <summary>
/// ID, PW�� �޾Ƽ� �ش� ������ ��Ī�Ǵ� ���� ������ Firebase�κ��� �������� Ŭ����.
/// </summary>
public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;

    // �ٸ� ��ũ��Ʈ���� AuthManager �ν��Ͻ� �����ϵ���.
    public static AuthManager Instance
    {
        get 
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    // ���� ���̳� ���� �����ϴ� ȯ���� Firebase�� ������ �� �ִ� ��Ȳ���� üũ.
    public bool IsFirebaseReady { get; private set; }
    // �α��� ������ �ߺ����� ���� �ʵ��� �α��� ���������� üũ.
    public bool IsSignInOnProgress { get; private set; }

    public InputField idField;
    public InputField passwordField;
    public Button signInButton;

    public FirebaseApp firebaseApp;
    public FirebaseAuth firebaseAuth;
    public FirebaseUser User;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        signInButton.interactable = false;

        // ���� ���̾�̽��� ������ �� �ִ� ȯ������ üũ.
        // Async: ���� ���ڸ��� �ϷḦ ��ٸ��� �ʰ� �ٷ� ���� �������� �Ѿ. -> �ݹ� or ü���� �ɾ�� ��.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var result = task.Result;

            // ���̾�̽� ������ �Ұ����� ����
            if (result != DependencyStatus.Available)
            {
                Debug.LogError(result.ToString());
                AuthManager.Instance.IsFirebaseReady = false;
            }
            // ���̾�̽� ������ ������ ����
            else
            {
                AuthManager.Instance.IsFirebaseReady = true;
                //firebase app, auth�� ��ü���� ����� �����ϴ� ������Ʈ�� ������.
                AuthManager.Instance.firebaseApp = FirebaseApp.DefaultInstance;
                AuthManager.Instance.firebaseAuth = FirebaseAuth.DefaultInstance;
            }

            signInButton.interactable = IsFirebaseReady;
        });
    }

    public void SignIn(string sceneName)
    {
        // ���̾�̽��� �غ� �ȵ� ����, �̹� �α��� �õ��� �� ����, ���� ������ �Ҵ� �� ����.
        if (!IsFirebaseReady || IsSignInOnProgress || AuthManager.Instance.User != null)
        {
            return;
        }

        IsSignInOnProgress = true;
        signInButton.interactable = false;

        // SignInWithCredentialAsync: �����̳� ���ð��� �� �ܺ� ���񽺸� ����ؼ� �α��� �Ҷ� ���.
        // ������ �ӽ÷� �̸��� �н������ ��.
        firebaseAuth.SignInWithEmailAndPasswordAsync(idField.text, passwordField.text).ContinueWithOnMainThread(task =>
        {
            IsSignInOnProgress = false;
            signInButton.interactable = true;

            if (task.IsFaulted)
            {
                Debug.LogError(task.Exception);
            }
            else if (task.IsCanceled)
            {
                Debug.LogError("Sign in canceled");
            }
            else
            {
                User = task.Result;
                Debug.Log(User.Email);
                SceneManager.LoadScene(sceneName);
            }
        });
    }
}
