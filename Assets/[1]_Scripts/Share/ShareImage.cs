using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
 
namespace BrakeBricks
{
  public class ShareImage : MonoBehaviour 
  {
    
  #region Var

    public static ShareImage Instance;

    private bool isProcessing = false;
    
    private string shareText  = "I'm playing an interesting game now.\n";
    private string gameLink = "Download the game on play store at \n"+ StaticPrm.LINK_GAME_STORE ;
    private string subject = "Brick balls war";
    private string imageName = "Image/shareImage"; // without the extension, for iinstance, MyPic 
    
  #endregion


  #region Event

    public event System.Action ShareGameEvent;

  #endregion


    void Awake()
    {
      if (Instance == null)
        Instance = this;
    }


    void Start()
    {
      InputManager.Instance.FriendsPanel_PressButtonShareEvent += ImageShare;
    }


    public void ImageShare()
    {
    
      //if(!isProcessing)
        //StartCoroutine( ShareScreenshot() );  
        
      if(!isProcessing)
        StartCoroutine( ShareGame() );  
           

      ShareGameEvent?.Invoke(); 
      
    }
  
    private IEnumerator ShareScreenshot()
    {
      isProcessing = true;
      yield return new WaitForEndOfFrame();
      
      Texture2D screenTexture = new Texture2D(1080, 1080,TextureFormat.RGB24,true);
      screenTexture.Apply();
    
      byte[] dataToSave = Resources.Load<TextAsset>(imageName).bytes;
      
      string destination = Path.Combine(Application.persistentDataPath,System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".png");
      Debug.Log(destination);
      File.WriteAllBytes(destination, dataToSave);
    
      if(!Application.isEditor)
      {
        shareText = GetDicsriptions();
        
        AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
        AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
        intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse","file://" + destination);
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), shareText + gameLink);
        intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
        intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");
        AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
          
        currentActivity.Call("startActivity", intentObject);
        
      }
      
      isProcessing = false;   
    
    } 


    private IEnumerator ShareGame()
    {
      isProcessing = true;

      yield return new WaitForEndOfFrame();

      Texture2D ss = new Texture2D( Screen.width, Screen.height, TextureFormat.RGB24, false );
      ss.ReadPixels( new Rect( 0, 0, Screen.width, Screen.height ), 0, 0 );
      ss.Apply();      
    
      byte[] dataToSave = Resources.Load<TextAsset>(imageName).bytes;      
      string filePath = Path.Combine(Application.persistentDataPath,System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".png");
      File.WriteAllBytes(filePath, dataToSave);
      
      // To avoid memory leaks
      Destroy( ss );

      shareText += GetDicsriptions() + "\n" + gameLink;
      new NativeShare().AddFile( filePath ).SetSubject( "Subject goes here" ).SetText( shareText ).Share();

      isProcessing = false;
    }


    //возвращает строковое описание достижений игрока
    string GetDicsriptions()
    {
      var game = GameManager.CurrentGame;

      return string.Format("My best achievements: RATING of stars - [{0}], " 
                    + "hi-score in a CLASSIC game - [{1}], " 
                    + "number of victories in MULTYPLAYER - [{2}]. ",  
                    game.RatingCount, game.HiscoreClassicMode, game.StatisticGame.NumWinsMultiplayer);
    }   
  }
}
