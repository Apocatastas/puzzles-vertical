using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;


public class GameControl : MonoBehaviour
{
    #region PuzzleControls
    enum STEP
    {
        NONE = -1,
        PLAYING = 0,
        CLEAR,
        NUM
    };

    public enum SE
    {
        NONE = -1,
        GRAB = 0,
        RELEASE,
        ATTACH,
        COMPLETE,
        BUTTON,
        NUM,
    };

    private STEP current_step = STEP.PLAYING;
#pragma warning disable 0414
    private STEP next_step = STEP.NONE;
#pragma warning restore 0414
    private float step_timer = .0f;
    #endregion PuzzleControls

    public GameObject winSpawner;
    public Canvas cam;
    public Text passIndicator;
    public GameObject switch_next;
    public GameObject prefab_puzzle;
    public PuzzleControl script_puzzle_control;
    public AudioSource soundHolder;
    public MeshRenderer[] renderers;
    public GameObject[] baloons;
    public Shader shader_diff;
    public Admob adholder;
    public Assets.SimpleLocalization.MenuController m_menuController;
    public GameObject epInterface;
    public GameObject mainInterface;
    public GameObject puzzleObject;
    public bool isArrow;

    public void Awake()
    {
        isArrow = false;
        Broadcaster.Game = this;
        soundHolder = Broadcaster.sounds;

        if (PlayerPrefs.HasKey("SoundStatus"))
        {
            switch (PlayerPrefs.GetInt("SoundStatus"))
            {
                case 1:
                    soundHolder.volume = 1;
                    break;
                case 0:
                    soundHolder.volume = 0;
                    break;
            }
        }
        if (!PlayerPrefs.HasKey("LevelSetupPassed"))
        {
            foreach (string key in Broadcaster.pref_num)
            {
                PlayerPrefs.SetInt(key, 0);
            }
            PlayerPrefs.SetInt("LevelSetupPassed", 1);
        }

        adholder = FindObjectOfType<Admob>();
        m_menuController = FindObjectOfType<Assets.SimpleLocalization.MenuController>();
        shader_diff = Shader.Find("Sprites/Default");
    }

    public void Disassemble()
    {
        SceneManager.UnloadSceneAsync(1);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        ClearBaloons();
    }


    public void MakePuzzle()
    {
        this.script_puzzle_control = (Instantiate(this.prefab_puzzle) as GameObject).GetComponent<PuzzleControl>();
        script_puzzle_control.transform.SetParent(cam.transform.parent);
        Broadcaster.Who = Broadcaster.Prefab.name;
        Broadcaster.lastPlayed = prefab_puzzle;
        StartCoroutine(ShaderRefresh());
    }
    void Start()
    {
        ClearGame();
        MakePuzzle();
        StartCoroutine("ShowPreAd");
    }
    public void PlayNextLevel()
    {
        isArrow = true;
        GetNextPrefab();
        SceneManager.UnloadSceneAsync(1);
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        ClearBaloons();
    }
    public void BackToMenu()
    {
        ClearBaloons();
        Broadcaster.From = prefab_puzzle.name;
        m_menuController.CallMainFromHome();
        SceneManager.UnloadSceneAsync(1);

    }

    public IEnumerator ShowPreAd()
    {
        if (!((Broadcaster.Prefab.name == "Dino2") ||
           (Broadcaster.Prefab.name == "Dino1") ||
           (Broadcaster.Prefab.name == "Dino3") ||
           (Broadcaster.Prefab.name == "Dog") ||
           (Broadcaster.Prefab.name == "Rabbit") ||
           (Broadcaster.Prefab.name == "Cat") ||
           (Broadcaster.Prefab.name == "Sheriff") ||
           (Broadcaster.Prefab.name == "Guido") ||
           (Broadcaster.Prefab.name == "Luigi")))

        {
            if (!isArrow)
            {
                yield return 0;
            }
            else
            {
                isArrow = false;
            }
        }

            Broadcaster.AdHolder.ShowInterstitial();
            Broadcaster.AdHolder.RequestInterstitial();
        yield return 0;

    }


    public IEnumerator ShowPostAd()
    {
                if ((Broadcaster.Prefab.name != "Dog") &&
                (Broadcaster.Prefab.name != "Rabbit") &&
                (Broadcaster.Prefab.name != "Dino1") &&
                (Broadcaster.Prefab.name != "Dino2") &&
                (Broadcaster.Prefab.name != "Guido") &&
                (Broadcaster.Prefab.name != "Luigi"))
                {
                    Broadcaster.AdHolder.ShowInterstitial();
                }
            
            yield return 0;

    }

    public void ClearGame()
    {
        GameObject[] toDel = GameObject.FindGameObjectsWithTag("Puzzle");
        foreach (GameObject temp in toDel)
        {
            Destroy(temp);
        }
        ClearBaloons();
        prefab_puzzle = Broadcaster.Prefab;
        switch_next.SetActive(true);
    }

    public void LevelPassed(string name)
    {
        if (PlayerPrefs.HasKey(name))
        {
            PlayerPrefs.SetInt(name, 1);
        }
    }

    void Update()
    {
        this.step_timer += Time.deltaTime;
        switch (this.current_step)
        {
            case STEP.PLAYING:
                if (this.script_puzzle_control.isCleared())
                {
                    this.next_step = STEP.CLEAR;
                }
                break;
        }


    }
    public void PlaySE(string what) //Sound processor (legacy)
    {
        foreach (AudioClip obj in script_puzzle_control.m_menuController.sounds)
        {
            if (obj.name == what)
            {
                script_puzzle_control.m_menuController.soundHolder.clip = (AudioClip)obj;
                script_puzzle_control.m_menuController.soundHolder.Play();
            }

        }
    }

    public void NextPrefabProcessor(int limit_all, int limit_free, string[] broadcaster_prefnum, string path)
    {
        int limit = 0;

        limit = limit_all;
               

        for (int i = 0; i < broadcaster_prefnum.Length; i++)
        {
            if (broadcaster_prefnum[i] == Broadcaster.Prefab.name)
            {
                if (i == limit)
                {
                    Broadcaster.Prefab = Resources.Load<GameObject>(path + broadcaster_prefnum[0]);
                    //if got blocked levels, return to first
                }
                else
                {
                    Broadcaster.Prefab = Resources.Load<GameObject>(path + broadcaster_prefnum[i + 1]);
                }
                return;
            }
        }
    }
    public void GetNextPrefab()
    {

        switch (Broadcaster.From)
        {
            case "ScrollaCars":
                NextPrefabProcessor(4, 4, Broadcaster.pref_num_cars, "Prefab/Cars/");
                break;
            case "ScrollaAnimals":
                NextPrefabProcessor(29, 29, Broadcaster.pref_num, "Prefab/Animals/");
                break;
            case "ScrollaDinos":
                NextPrefabProcessor(4, 4, Broadcaster.pref_num_dinos, "Prefab/Dinos/");
                break;
        }

    }
    public void ClearBaloons()
    {
        baloons = GameObject.FindGameObjectsWithTag("Baloon");
        foreach (GameObject baloon in baloons)
        {
            Destroy(baloon);
        }
    }
    public IEnumerator ShaderRefresh() //fix for undesired transparency bug
    {
        yield return new WaitForSeconds(0.05f);
        renderers = script_puzzle_control.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer temp in renderers)
        {
            temp.sharedMaterial.shader = shader_diff;
        }
        yield return true;
    }

}