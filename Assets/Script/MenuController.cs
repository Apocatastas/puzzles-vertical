using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using com.adjust.sdk;
using System.Globalization;


namespace Assets.SimpleLocalization
{
    public class MenuController : MonoBehaviour
    {
        public GameObject uiCanvas; //for disable all controls between scenes
        public GameObject[] toggles;
        public AudioSource musicHolder;
        public AudioSource soundHolder;
        public Object[] sounds;
        public GameObject musicToggle;
        public GameObject soundToggle;
        public Text versioniser;
        public Admob admob;
        public GameObject mainInterface;
        public GameObject[] main_modes;

        public void Awake()
        {
            //DEBUG TO DEL
            PlayerPrefs.SetInt("OTHELLOSHEN", 0);
            PlayerPrefs.SetInt("MusicStatus", 0);
            PlayerPrefs.SetInt("SoundStatus", 0);
            //DEBUG TO DEL


            Broadcaster.From = "Divider";

            InitialPrefsSetting();
            uiCanvas.SetActive(true); //enable all main scene conrols
            SwitchToMainFromEP();
            admob.RequestInterstitial();
            sounds = Resources.LoadAll("Sound", typeof(AudioClip));
            Broadcaster.sounds = soundHolder.GetComponent<AudioSource>();
        }
       
        private void Start()
        {
            LaunchSoundcheck();
            LocalizationManager.Read();
            versioniser.text = "ver. " + Application.version;
            LangCheck();
            AmpEvent("Session_start");
           
        }

        public void LevelPush()
        {
            string one = "";

            switch (WhoIsIt(Broadcaster.Prefab.name))
            {
                case "Animal":
                    one = "z";
                    break;
                case "Car":
                    one = "m";
                    break;
                case "Dino":
                    one = "d";
                    break;
            }

            Dictionary<string, object> demoOptions = new Dictionary<string, object>() { {"success_tap", one+GetPuzzleNum(Broadcaster.Prefab.name) } };
            Amplitude.Instance.logEvent("success_tap", demoOptions);

        }

        public int GetPuzzleNum(string who)
       {
            foreach (var pair in Broadcaster.PuzzlesNum)
            {
                if (pair.Key == who)
                {
                    return pair.Value;
                }
            }
            return 0;
        }

        public void PlayLevel(GameObject prefabName)
        {

            Broadcaster.AdHolder = admob;
            Broadcaster.Prefab = prefabName;
            SceneManager.LoadScene(1, LoadSceneMode.Additive);
            uiCanvas.SetActive(false);
        }

        public void SwitchToMain(string mode)
        {
            uiCanvas.SetActive(true);

            if (PlayerPrefs.HasKey("SoundStatus"))
            {
                switch (PlayerPrefs.GetInt("SoundStatus"))
                {
                    case 1:
                        if (mode == "Divider") { PlayShot("mainScreen"); }

                        break;
                    case 0:
                        break;
                }
            }

            mainInterface.SetActive(true);

            //Switch off undesired scrollsnaps
            foreach (GameObject main_mode in main_modes)
            {
                if (main_mode.name == mode)
                {

                    main_mode.SetActive(true);
                }
                else
                {

                    main_mode.SetActive(false);
                }
            }
            Broadcaster.From = mode;

        }

        public void SwitchToMainFromEP()
        {
             if (Broadcaster.isEPFromGame)
            {

                uiCanvas.SetActive(false);
                foreach (GameObject main_mode in main_modes)
                {
                    main_mode.SetActive(false);
                }
                PlayLevel(Broadcaster.lastPlayed);
            }
            else
            {

                SwitchToMain(Broadcaster.From);
            }


        }

        public string GetMainMode()
        {
            string m_mode = "";
            foreach (GameObject main_mode in main_modes)
            {
                if (main_mode.activeInHierarchy)
                { m_mode = main_mode.name; }
            }

            if (m_mode == "") { m_mode = Broadcaster.From; }

            return m_mode;
        }


        public void CloseToMain()
        {

            if (GetMainMode() == "Divider")
            {
                Leave();
            }
            else
            {
                SwitchToMain("Divider");
            }
            

        }

        public void Leave()
        {
            StopAllCoroutines();
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            Application.Quit();
        }
        
        public void SwitchToggle(GameObject toggle)
        {
            Text toggle_label;
            Color m_transparent;
            Color m_highlighted;

            foreach (GameObject t in toggles)
            {
                toggle_label = t.GetComponentInChildren<Text>();
                m_highlighted = toggle_label.color;
                m_transparent = toggle_label.color;

                if (t.name == toggle.name)
                {
                    t.GetComponent<Toggle>().isOn = true;
                    m_highlighted.a = 1f;
                    toggle_label.color = m_highlighted;
                }
                else
                {
                    t.GetComponent<Toggle>().isOn = false;
                    m_transparent.a = 0.5f;
                    toggle_label.color = m_transparent;
                }
            }
        }

        public void InitialPrefsSetting()
        {
            Broadcaster.isEPFromGame = false;

            if (!(PlayerPrefs.HasKey("OTHELLOSHEN")))
            {
                AmpEvent("First_launch");
                PlayerPrefs.SetInt("OTHELLOSHEN", 1);
            }

            if (!(PlayerPrefs.HasKey("MusicStatus")))
            {
                PlayerPrefs.SetInt("MusicStatus", 1);
            }

            if (!(PlayerPrefs.HasKey("SoundStatus")))
            {
                PlayerPrefs.SetInt("SoundStatus", 1);
            }

        }

        public AudioClip SoundChooser(string what)
        {
            foreach (Object obj in sounds)
            {
                if (obj.name == what)
                {
                    return (AudioClip)obj;
                }

            }
            return (AudioClip)sounds[0];
        }

        public void PlayShot(string what)
        {
            soundHolder.GetComponent<AudioSource>().clip = SoundChooser(what);
            soundHolder.GetComponent<AudioSource>().Play();
        }

        public void PlayMusic()
        {
            musicHolder.GetComponent<AudioSource>().clip = SoundChooser("soundtrack");
            musicHolder.GetComponent<AudioSource>().Play();
        }

        public void WindowClose()
        {
            PlayShot("windowClose");
        }

        public void PostmanCaller()
        {
            Application.OpenURL("mailto:dmitriyvoln@wsoteam.com");

        }

        public void Review()
        {
            Application.OpenURL("https://play.google.com/store/apps/details?id=com.WSO.FunnyAnimalsPuzzles");
        }

        public string WhoIsIt(string prefab)
        {
            if (
                (prefab == "Dog") ||
                (prefab == "Rabbit") ||
                (prefab == "Cat") ||
                (prefab == "Hengehog") ||
                (prefab == "Cow") ||
                (prefab == "Donkey") ||
                (prefab == "Eagle") ||
                (prefab == "Sloth") ||
                (prefab == "Whale") ||
                (prefab == "Fish") ||
                (prefab == "Panda") ||
                (prefab == "Parrot") ||
                (prefab == "Tiger") ||
                (prefab == "Beer") ||
                (prefab == "Cheetah") ||
                 (prefab == "Jiraffe") ||
                 (prefab == "Penguin") ||
                 (prefab == "Leo") ||
                 (prefab == "Lemur") ||
                 (prefab == "Pig") ||
                 (prefab == "Elephant") ||
                (prefab == "Fox") ||
                (prefab == "Owl") ||
                (prefab == "Squirrel") ||
                (prefab == "Hamster") ||
                (prefab == "Bat") ||
                (prefab == "Racoon") ||
                (prefab == "Tortoise") ||
                (prefab == "Monkey") ||
                (prefab == "Chameleon")
                )
            {
                return "Animal";
            }

            if (
                (prefab == "Luigi") ||
                (prefab == "Guido") ||
                (prefab == "Monty") ||
                (prefab == "Sally") ||
                (prefab == "Sheriff")
                )
            {
                return "Car";
            }

            if (
                (prefab == "Dino1") ||
                (prefab == "Dino2") ||
                (prefab == "Dino3") ||
                (prefab == "Dino4") ||
                (prefab == "Dino5")
                )
            {
                return "Dino";
            }

            return "";
        }

        public void CallMainFromHome()
        {

            switch (WhoIsIt(Broadcaster.From))
            {
                case "Animal":
                    Broadcaster.From = "ScrollaAnimals";

                    break;
                case "Car":
                    Broadcaster.From = "ScrollaCars";

                    break;
                case "Dino":
                    Broadcaster.From = "ScrollaDinos";

                    break;
                default:
                    Broadcaster.From = "Divider";

                    break;
            }
            Broadcaster.isEPFromGame = false;
            SwitchToMainFromEP();

        }


        #region SoundSetting


        public void PlayVoice(string who)
        {
            switch (PlayerPrefs.GetInt("SoundStatus"))
            {
                case 0:
                    break;
                case 1:
                    PlayShot(who + "_new");
                    break;
                default:
                    break;
            }
        }
        public void LaunchSoundcheck()
        {
            ReadMusicPrefs();
            ReadSoundPrefs();
        }
        public void ReadMusicPrefs()
        {
            switch (PlayerPrefs.GetInt("MusicStatus"))
            {
                case 1:
                    MusicOn();
                    break;
                case 0:
                    MusicOff();
                    break;
            }
        }
        public void SwitchMusicPrefs()
        {
            if (PlayerPrefs.HasKey("MusicStatus"))
            {
                switch (PlayerPrefs.GetInt("MusicStatus"))
                {
                    case 0:
                        MusicOn();
                        break;
                    case 1:
                        MusicOff();
                        break;
                }
            }

        }
        public void ReadSoundPrefs()
        {
            switch (PlayerPrefs.GetInt("SoundStatus"))
            {
                case 1:
                    SoundOn();
                    break;
                case 0:
                    SoundOff();
                    break;
            }
        }
        public void SwitchSoundPrefs()
        {
            if (PlayerPrefs.HasKey("SoundStatus"))
            {
                switch (PlayerPrefs.GetInt("SoundStatus"))
                {
                    case 0:
                        SoundOn();
                        break;
                    case 1:
                        SoundOff();
                        break;
                }
            }

        }
        public void SoundOn()
        {
            soundToggle.GetComponent<Toggle>().isOn = true;
            soundHolder.GetComponent<AudioSource>().mute = false;
            PlayerPrefs.SetInt("SoundStatus", 1);
        }
        public void SoundOff()
        {
            soundToggle.GetComponent<Toggle>().isOn = false;
            soundHolder.GetComponent<AudioSource>().mute = true;
            PlayerPrefs.SetInt("SoundStatus", 0);
        }
        public void MusicOn()
        {
            musicToggle.GetComponent<Toggle>().isOn = true;
            musicHolder.GetComponent<AudioSource>().clip = SoundChooser("soundtrack");
            musicHolder.GetComponent<AudioSource>().Play();
            musicHolder.GetComponent<AudioSource>().mute = false;
            PlayerPrefs.SetInt("MusicStatus", 1);
        }
        public void MusicOff()
        {
            musicToggle.GetComponent<Toggle>().isOn = false;
            musicHolder.GetComponent<AudioSource>().clip = SoundChooser("soundtrack");
            musicHolder.GetComponent<AudioSource>().Pause();
            musicHolder.GetComponent<AudioSource>().mute = true;
            PlayerPrefs.SetInt("MusicStatus", 0);
        }

        #endregion SoundSetting

        #region Localization
        public void SetLocalization(string localization)
        {
            LocalizationManager.Language = localization;
            PlayerPrefs.SetString("Lang", localization);
        }
        public void LangCheck()
        {
            string m_langtoggle = "";

            if (PlayerPrefs.HasKey("Lang"))
            {
                SetLocalization(PlayerPrefs.GetString("Lang"));
                m_langtoggle = PlayerPrefs.GetString("Lang") + "Toggle";


            }
            else
            {
                SwitchLanguage();
                m_langtoggle = Application.systemLanguage + "Toggle";
            }
            foreach (GameObject t in toggles)
            {
                if (t.name == m_langtoggle)
                {
                    SwitchToggle(t);
                }
            }

        }
        public void SwitchLanguage()
        {
            switch (Application.systemLanguage)
            {
                case SystemLanguage.Russian:
                    LocalizationManager.Language = "Russian";
                    PlayerPrefs.SetString("Lang", "Russian");

                    break;
                case SystemLanguage.English:
                    LocalizationManager.Language = "English";
                    PlayerPrefs.SetString("Lang", "English");

                    break;
                case SystemLanguage.Spanish:
                    LocalizationManager.Language = "Spanish";
                    PlayerPrefs.SetString("Lang", "Spanish");

                    break;
                case SystemLanguage.Portuguese:
                    LocalizationManager.Language = "Portuguese";
                    PlayerPrefs.SetString("Lang", "Portuguese");

                    break;
                case SystemLanguage.Indonesian:
                    LocalizationManager.Language = "Hindi";
                    PlayerPrefs.SetString("Lang", "Hindi");

                    break;
                default:
                    LocalizationManager.Language = "English";
                    PlayerPrefs.SetString("Lang", "English");

                    break;
            }
        }
        #endregion Localization

        public void AmpEvent(string what)
        {
            Amplitude.Instance.logEvent(what);
        }
 
        public void SendAdjustEvent(string what)
        {
            AdjustEvent adjustEvent = new AdjustEvent(what);
            Adjust.trackEvent(adjustEvent);
        }

    }
}