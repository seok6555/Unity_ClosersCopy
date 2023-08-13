using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;

/// <summary>
/// ID, PW를 받아서 해당 계정에 매칭되는 계정 정보를 Firebase로부터 가져오는 클래스.
/// </summary>
public class AuthManager : MonoBehaviour
{
    private static AuthManager instance;

    // 다른 스크립트에서 AuthManager 인스턴스 참조하도록.
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

    // 현재 앱이나 현재 실행하는 환경이 Firebase를 구동할 수 있는 상황인지 체크.
    public bool IsFirebaseReady { get; private set; }
    // 로그인 진행이 중복으로 되지 않도록 로그인 진행중인지 체크.
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

        // 현재 파이어베이스를 구동할 수 있는 환경인지 체크.
        // Async: 실행 하자마자 완료를 기다리지 않고 바로 다음 라인으로 넘어감. -> 콜백 or 체인을 걸어야 함.
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var result = task.Result;

            // 파이어베이스 구동이 불가능한 상태
            if (result != DependencyStatus.Available)
            {
                Debug.LogError(result.ToString());
                AuthManager.Instance.IsFirebaseReady = false;
            }
            // 파이어베이스 구동이 가능한 상태
            else
            {
                AuthManager.Instance.IsFirebaseReady = true;
                //firebase app, auth의 전체적인 기능을 관리하는 오브젝트를 가져옴.
                AuthManager.Instance.firebaseApp = FirebaseApp.DefaultInstance;
                AuthManager.Instance.firebaseAuth = FirebaseAuth.DefaultInstance;
            }

            signInButton.interactable = IsFirebaseReady;
        });
    }

    public void SignIn(string sceneName)
    {
        // 파이어베이스가 준비 안된 상태, 이미 로그인 시도를 한 상태, 유저 정보가 할당 된 상태.
        if (!IsFirebaseReady || IsSignInOnProgress || AuthManager.Instance.User != null)
        {
            return;
        }

        IsSignInOnProgress = true;
        signInButton.interactable = false;

        // SignInWithCredentialAsync: 구글이나 애플계정 등 외부 서비스를 사용해서 로그인 할때 사용.
        // 지금은 임시로 이메일 패스워드로 함.
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
