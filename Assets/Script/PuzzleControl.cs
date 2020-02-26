using UnityEngine;

using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class PuzzleControl : MonoBehaviour
{

    public GameControl script_game_control = null;
    public Assets.SimpleLocalization.MenuController m_menuController;
    private int piece_num;
    private int piece_num_finished;
    public List<int> m_newIndexes = new List<int>();

    enum STEP
    {
        NONE = -1,
        PLAY = 0,
        CLEAR,
        NUM
    }

    private STEP step_now = STEP.NONE;
    private STEP step_next = STEP.NONE;

    private bool isWin;
    private float step_time = .0f;
    private float step_time_prev = .0f;
    private PieceControl[] pieces_all;
    private PieceControl[] pieces_active;
    private bool is_clear_show = false;
    public int hardcodeWin;

    public AudioClip SoundChooser(string what)
    {
        foreach (AudioClip obj in m_menuController.sounds)
        {
            if (obj.name == what)
            {
                return (AudioClip)obj;
            }

        }

 
           return (AudioClip)m_menuController.sounds[0];
    }

    public void PlayShot(string what)
    {
        m_menuController.soundHolder.clip = SoundChooser(what);
        m_menuController.soundHolder.Play();
    }

    public void InitPieces()
    {
        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject _piece = this.transform.GetChild(i).gameObject;
            if (this.isPiece(_piece))
            {
                this.piece_num++;
            }
        }

        this.pieces_all = new PieceControl[this.piece_num];
        this.pieces_active = new PieceControl[this.piece_num];

        for (int i = 0, n = 0; i < this.transform.childCount; i++)
        {
            GameObject _piece = this.transform.GetChild(i).gameObject;
            if (!this.isPiece(_piece))
            {
                continue;
            }
            _piece.AddComponent<PieceControl>();
            _piece.GetComponent<PieceControl>().script_puzzle_control = this;
            this.pieces_all[n++] = _piece.GetComponent<PieceControl>();
        }

        this.piece_num_finished = 0;

        for (int i = 0; i < this.transform.childCount; i++)
        {
            GameObject _baseObj = this.transform.GetChild(i).gameObject;
            if (this.isPiece(_baseObj))
            {
                continue;
            }
            _baseObj.GetComponent<Renderer>().material.renderQueue = this.getDrawPriorityBase();
        }
    }

    void Start()
    {

        isWin = false;
        hardcodeWin = 0; //legacy
        m_menuController = FindObjectOfType<Assets.SimpleLocalization.MenuController>();
        this.script_game_control = Broadcaster.Game;
        this.piece_num = 0;
        PlayShot("puzzleShuffle");
        InitPieces();
        PlacePieces();
        this.is_clear_show = false;
    }

    void Celebration()
    {
        StartCoroutine(ShakeIt());
        script_game_control.winSpawner.GetComponent<Spawner>().Win();
        isWin = true;

    }

    void CheckPuzzleStatus()
    {
        this.step_time_prev = this.step_time;
        this.step_time += Time.deltaTime;

        switch (this.step_now)
        {
            case STEP.NONE:
                this.step_next = STEP.PLAY;
                break;
            case STEP.PLAY:
                if (this.piece_num_finished >= this.piece_num)
                {
                    this.step_next = STEP.CLEAR;
                }
                break;
        }

        while (this.step_next != STEP.NONE)
        {
            this.step_now = this.step_next;
            this.step_next = STEP.NONE;
            this.step_time = .0f;
            switch (this.step_now)
            {
                case STEP.PLAY:
                    for (int i = 0; i < this.pieces_all.Length; i++)
                    {
                        this.pieces_active[i] = this.pieces_all[i];
                    }
                    this.piece_num_finished = 0;
                    foreach (PieceControl _piece in this.pieces_all)
                    {
                        _piece.restart();
                    }
                    break;
                case STEP.CLEAR:
                    break;
            }
        }

        switch (this.step_now)
        {
            case STEP.CLEAR:
                const float clear_delay = 0.5f;
                if (this.step_time_prev < clear_delay && this.step_time >= clear_delay)
                {
                    this.script_game_control.PlaySE("applause");
                    this.is_clear_show = true;
                }
                break;
        }
    }

    void Update()
    {
        CheckPuzzleStatus();

        if (!isWin)
        {
            if (hardcodeWin > 6)
            {
                foreach (var pair in Broadcaster.PuzzlesPiecesToWin)
                {
                    if (pair.Key == script_game_control.prefab_puzzle.name)
                    {
                        if (pair.Value == hardcodeWin)
                        { Celebration(); }
                    }
                }
            }
           
        }


    }

    public void pickPiece(PieceControl _piece) //
    {

    }

    public void finishPiece(PieceControl _piece)
    {

    }

    private bool isPiece(GameObject obj)
    {
        bool ret = false;
        if (obj.name.Contains("base"))
        {
            ret = false;
        }
        else
        {
            ret = true;
        }
        return ret;
    }

    private void setPiecesHeightOffset()
    {

        float offset = 0.1f;
        int n = 0;
        foreach (PieceControl _piece in this.pieces_all)
        {
            if (_piece == null)
            {
                continue;
            }
            _piece.GetComponent<Renderer>().material.renderQueue = this.getDrawPriorityPiece(n);
            offset += 0.1f / this.piece_num;
            _piece.setHeightOffset(offset);
            n++;
        }
    }

    private int getDrawPriorityBase()
    {
        return 0;
    }

    private int getDrawPriorityFinishPiece()
    {
        int priority = 0;
        priority = this.getDrawPriorityBase() + 1;
        return priority;
    }

    private int getDrawPriorityPiece(int priorityInPieces)
    {
        int priority = 0;
        priority = this.getDrawPriorityFinishPiece() + 2;
        priority += this.piece_num - 1 - priorityInPieces;
        return priority;
    }

    private void PlacePieces()


    {
        List<int> m_unsortIndexes = new List<int>();

        //implementing Fisher-Yates algorithm to shuffle pieces
        for (int i = 0; i < pieces_all.Length; i++)
        {
            m_unsortIndexes.Add(i);
        }
        int numHolder;
        int counter = m_unsortIndexes.Count;
        int j = 0;
        while (m_newIndexes.Count < counter)
        {
            if (j < counter)
            {
                numHolder = Mathf.CeilToInt(Random.Range(0, m_unsortIndexes.Count));
                m_newIndexes.Add(m_unsortIndexes[numHolder]);
                m_unsortIndexes.Remove(m_unsortIndexes[numHolder]);
                j++;
            }
        }

        //Place pieces on the circle

        for (int i = 0; i < pieces_all.Length; i++)
        {
            switch (i)
            {
                case 0:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-4.5f, 1f, pieces_all[i].pos_begin.z);
                    break;
                case 1:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(3f, 1.5f, pieces_all[i].pos_begin.z);
                    break;
                case 2:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-4.5f, -1f, pieces_all[i].pos_begin.z);
                    break;
                case 3:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(3f, -1f, pieces_all[i].pos_begin.z);
                    break;
                case 4:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-2.5f, 2.5f, pieces_all[i].pos_begin.z);
                    break;
                case 5:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-2.5f, -2.5f, pieces_all[i].pos_begin.z);
                    break;
                case 6:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(0.8f, 2.5f, pieces_all[i].pos_begin.z);
                    break;
                case 7:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(0.8f, -2.5f, pieces_all[i].pos_begin.z);
                    break;

                case 8:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(1.5f, 1.5f, pieces_all[i].pos_begin.z);
                    break;
                case 9:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(1.5f, -1.8f, pieces_all[i].pos_begin.z);
                    break;
                case 10:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-2.5f, -1.8f, pieces_all[i].pos_begin.z);
                    break;
                case 11:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-2.5f, 1.5f, pieces_all[i].pos_begin.z);
                    break;
                case 12:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-2.5f, -1f, pieces_all[i].pos_begin.z);
                    break;
                case 13:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(0f, 2.5f, pieces_all[i].pos_begin.z);
                    break;
                case 14:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(0f, -2.5f, pieces_all[i].pos_begin.z);
                    break;
                case 15:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(3.2f, 0f, pieces_all[i].pos_begin.z);
                    break;
                case 16:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-4.7f, 0f, pieces_all[i].pos_begin.z);
                    break;
                case 17:
                    pieces_all[m_newIndexes[i]].pos_begin = new Vector3(-2.5f, 0.8f, pieces_all[i].pos_begin.z);
                    break;





            }

        }
    }

    public bool isCleared()
    {
        return this.step_now == STEP.CLEAR;
    }

    public bool isFinishShow()
    {
        return this.is_clear_show;
    }

    IEnumerator ShakeIt()
    {
        script_game_control.LevelPassed(Broadcaster.Prefab.name);

        if (
            (PlayerPrefs.GetInt("Dog") == 1) &&
            (PlayerPrefs.GetInt("Rabbit") == 1) &&
            (PlayerPrefs.GetInt("Cat") == 1) &&
            (PlayerPrefs.GetInt("Hengehog") == 1) &&
            (PlayerPrefs.GetInt("Cow") == 1) &&
            (PlayerPrefs.GetInt("Donkey") == 1) &&
            (PlayerPrefs.GetInt("Eagle") == 1) &&
            (PlayerPrefs.GetInt("Sloth") == 1) &&
            (PlayerPrefs.GetInt("Whale") == 1) &&
            (PlayerPrefs.GetInt("Fish") == 1) &&
            (PlayerPrefs.GetInt("Panda") == 1) &&
            (PlayerPrefs.GetInt("Parrot") == 1) &&
            (PlayerPrefs.GetInt("Dino1") == 1) &&
            (PlayerPrefs.GetInt("Dino2") == 1) &&
            (PlayerPrefs.GetInt("Dino3") == 1) &&
            (PlayerPrefs.GetInt("Dino4") == 1) &&
            (PlayerPrefs.GetInt("Dino5") == 1) &&
            (PlayerPrefs.GetInt("Sally") == 1) &&
            (PlayerPrefs.GetInt("Monty") == 1) &&
            (PlayerPrefs.GetInt("Sheriff") == 1) &&
            (PlayerPrefs.GetInt("Luigi") == 1) &&
            (PlayerPrefs.GetInt("Guido") == 1)
            )
        {
            PlayerPrefs.SetInt("FreeLevelsPassed", 1);
        }



        if (PlayerPrefs.HasKey("SoundStatus"))
        {
            switch (PlayerPrefs.GetInt("SoundStatus"))
            {
                case 0:
                    break;
                case 1:
                    PlayShot(script_game_control.prefab_puzzle.name + "_new");
                    yield return new WaitForSeconds(3f);
                    PlayShot("applause");
                    break;
                default:
                    break;
            }

        }





        if (
            (script_game_control.prefab_puzzle.name == "Pig") ||
            (script_game_control.prefab_puzzle.name == "Dino1") ||
            (script_game_control.prefab_puzzle.name == "Dino2") ||
            (script_game_control.prefab_puzzle.name == "Dino3") ||
            (script_game_control.prefab_puzzle.name == "Dino4") ||
            (script_game_control.prefab_puzzle.name == "Dino5") ||
            (script_game_control.prefab_puzzle.name == "Sally") ||
            (script_game_control.prefab_puzzle.name == "Monty") ||
            (script_game_control.prefab_puzzle.name == "Sheriff") ||
            (script_game_control.prefab_puzzle.name == "Luigi") ||
            (script_game_control.prefab_puzzle.name == "Guido")


            )
        {
            for (int i = 1; i < 10; i++)
            {
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), 10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), 10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), 10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 1, 0), 10f);
            }
        }
        else
        {
            for (int i = 1; i < 10; i++)
            {
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), 10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), 10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), 10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), -10f);
                yield return new WaitForSeconds(0.05f);
                transform.Rotate(new Vector3(0, 0, 1), 10f);
            }
        }
        script_game_control.StartCoroutine("ShowPostAd");
        yield return true;
    }

}