using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class DiamondGame : MonoBehaviour
{

    public GameObject ShortLaserPrefab;
    public GameObject LongLaserPrefab;
    public GameObject BackwardLaserPrefab;
    public GameObject DiamondPrefab;
    public GameObject LaserLinePrefab;
    public GameObject LaserBoxPrefab;
    public GameObject HaloPrefab;
    public GameObject retryTargetPrefab;
    //public ThermometerManager thermometerManager;
    public GameObject ThermoT0_Prefab;
    public GameObject ThermoT1_Prefab;
    public GameObject ThermoT2_Prefab;
    public GameObject ThermoT3_Prefab;
    public GameObject ThermoT4_Prefab;
    public GameObject ThermoT5_Prefab;
    public GameObject ThermoT6_Prefab;
    public GameObject ThermoT7_Prefab;
    public GameObject ThermoT8_Prefab;
    public GameObject ThermoT9_Prefab;
    public GameObject ThermoT10_Prefab;
    public GameObject ThermoContainer_Prefab;
    private GameObject[] Thermometer = new GameObject[12];
    private int TemperatureDegree = 0;

    //public GameOverMessage gameOverMessage;
    public GameObject gameOverMessagePrefab;
    private GameObject gameOverMessage;

    public TMPro.TextMeshPro LevelMsg;
    public AudioClip DeletePulseClip, PulseHitLaserClip, GameOverClip, AlarmClip;

    private AudioSource audioSource;
    private HighScoreKeeper highScoreKeeper;
    private ViewpointManager viewpointManager;
    public string sceneToLoad = "Viewpoints";

    public TextMesh errorLog;
    private TextMesh errorLog2;

    private const int PulseQueueSize = 3000;
    public PusleStruct[] PulseQueue = new PusleStruct[PulseQueueSize];

    private int[] LongPulsesPositions = new int[50];
    private int LongPulsePos = 0;
    private GameObject[] BackwardPulses = new GameObject[5];
    private int[] BackwardPulsesPositions = new int[5];
    private int BackwardPulsePos = 0;
    private GameObject Diamond;
    private GameObject  LaserLine, LaserBox, Halo;
    private GameObject retryTarget;
    private int score = 0;

    private const float  offSetShort = 0.5f, offSetLong = 1.0f;//8.0f;//4.0f;//1.50f;
    private float offSet = 0.5f;
    private float MaxSpeed = 0.5f, Speed = 0.04f;//0.05f;//0.010f;
    private int nbr_S = 0, nbr_L = 0, nbr_B = 0;
    private int rnd_S = 0, rnd_L = 0, rnd_B = 0;
    private int nbrHitGreenPulse = 0, MaxnbrHitGreenPulse = 30;
    private const float StartPositionX = -15;//-20;//-30;//-20;//-1*PulseQueueSize / 2;
    private const float EndPosition = 13;//8;//30;//20;//(PulseQueueSize + StartPosition) * offSet - offSet;
    private int LaunchPoint = 0, LaunchPointBackward = 0;
    private int StartPositionY = 0;

    private bool ReachPosition = true;
    private float ReachOffSet = 0.0f;
    private bool PulseCreated = false;
    private bool GameOverStatus = false;
    private float Temperature = 1;
    private int nbrDestroyedBackwardPulse = 0, nbrDestroyedLongPulse = 0;
    private bool BackwardSync = false, Sync = false;
    private bool HitLaserBox = false, HaloActive = false;
    private int nbrSignal = 0;
    private bool isMine = true;
    NetworkView nw;

    private PULSE_GENDER CurrentPulse;
    public struct PusleStruct
    {
        public GameObject Pulse;
        public PULSE_GENDER Gender;
    };

    public enum PULSE_GENDER
    {
        SHORT = 0,
        LONG = 1,
        BACKWARD = 2
    };


    private void Awake()
    {
        nw = GetComponent<NetworkView>();
        if (nw != null && !nw.isMine)
            isMine = false;
    }

    // Use this for initialization
    void Start()
    {
        InvokeRepeating("UpdateSpeed", 19.0f, 15.0f);
        //InvokeRepeating("Alarm", 2.0f, 0.5f);
        score = 0;
        audioSource = gameObject.GetComponent<AudioSource>();

        for (int j = 0; j < LongPulsesPositions.Length; j++)
        {
            LongPulsesPositions[j] = -1;
        }

        for (int j = 0; j < BackwardPulsesPositions.Length; j++)
        {
            BackwardPulsesPositions[j] = -1;
        }

        viewpointManager = GameObject.Find("ViewpointManager").GetComponent<ViewpointManager>();
        if (viewpointManager == null)
        {
            Debug.LogError("GameController: Can't find ViewpointManager!");
            return;
        }

        GameObject obj = GameObject.Find("NewHightScroeKeeper");
        if (obj != null)
        {
            highScoreKeeper = obj.GetComponent<HighScoreKeeper>();
        }


        //LaserCreation();
        //DiamondCreation();

        //if (VraSettings.instance.isHeadset)
        //{
        //    LaunchPulse();
        //    RetryButtonCreation();
        //    //LaserCreation();
        //    //DiamondCreation();
        //}

        if (VraSettings.instance.isHeadset)
        {
            //if (gameOverMessage != null)
            //    Network.Destroy(gameOverMessage);
            InvokeRepeating("Alarm", 2.0f, 0.5f);
            LaunchPulse();
            RetryButtonCreation();
            LaserCreation();
            DiamondCreation();
            ThermometerCreation();
        }

    }

    // Update is called once per frame
    void Update()
    {
            if (!GameOverStatus)
        {
            //if (gameOverMessage != null)
            //    Network.Destroy(gameOverMessage);

            if (VraSettings.instance.isHeadset)
            {
                PulseCreation();                                        
            }

            PulseMovement();


            if (TemperatureDegree > 10)
            {
                audioSource.PlayOneShot(GameOverClip);
                if (VraSettings.instance.isHeadset)
                {
                    GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
                    if (nws != null)
                    {
                        nws.GetComponent<NetworkSync>().UpdateScoreTablet(score);
                    }
                }
           
                // Do we have a winner?
               // if (VraSettings.instance.isTablet)
               // {
                    ////GameObject obj = GameObject.Find("HighScoreKeeper");
                    ////if (obj != null)
                    ////{
                    ////    HighScoreKeeper hsk = obj.GetComponent<HighScoreKeeper>();
                    ////    if (hsk.isNewHighScore(score))
                    ////    {
                    ////        hsk.NewHighScore(score);
                    ////    }
                    ////}
                  //  highScoreKeeper.NewHighScore(score);
               // }
                
                GameOver();

                retryTarget.SetActive(true);
            }
        }
    }

    public void RetryButtonCreation()
    {
        if(retryTarget == null)
        {
            retryTarget = GameObject.Instantiate(retryTargetPrefab);
            retryTarget.transform.SetParent(this.gameObject.transform);
            retryTarget.SetActive(false);
            retryTarget.GetComponent<GazeTarget>().SetLabel("Play again");
        }


    }

    public void ThermometerCreation()
    {

        if (Thermometer[0] != null)
            Network.Destroy(Thermometer[0]);

        Thermometer[0] = (GameObject)Network.Instantiate(ThermoT0_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[0].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(1.5f, -2, 4));

        if (Thermometer[1] != null)
            Network.Destroy(Thermometer[1]);

        Thermometer[1] = (GameObject)Network.Instantiate(ThermoT1_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[1].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(1.229764f, -2, 4));

        if (Thermometer[2] != null)
            Network.Destroy(Thermometer[2]);

        Thermometer[2] = (GameObject)Network.Instantiate(ThermoT2_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[2].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.9595285f, -2, 4));

        if (Thermometer[3] != null)
            Network.Destroy(Thermometer[3]);

        Thermometer[3] = (GameObject)Network.Instantiate(ThermoT3_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[3].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.6892928f, -2, 4));

        if (Thermometer[4] != null)
            Network.Destroy(Thermometer[4]);

        Thermometer[4] = (GameObject)Network.Instantiate(ThermoT4_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[4].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.419057f, -2, 4));

        if (Thermometer[5] != null)
            Network.Destroy(Thermometer[5]);

        Thermometer[5] = (GameObject)Network.Instantiate(ThermoT5_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[5].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.1488212f, -2, 4));

        if (Thermometer[6] != null)
            Network.Destroy(Thermometer[6]);

        Thermometer[6] = (GameObject)Network.Instantiate(ThermoT6_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[6].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.1214144f, -2, 4));

        if (Thermometer[7] != null)
            Network.Destroy(Thermometer[7]);

        Thermometer[7] = (GameObject)Network.Instantiate(ThermoT7_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[7].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.3916502f, -2, 4));

        if (Thermometer[8] != null)
            Network.Destroy(Thermometer[8]);

        Thermometer[8] = (GameObject)Network.Instantiate(ThermoT8_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[8].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.661886f, -2, 4));

        if (Thermometer[9] != null)
            Network.Destroy(Thermometer[9]);

        Thermometer[9] = (GameObject)Network.Instantiate(ThermoT9_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[9].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.9321218f, -2, 4));

        if (Thermometer[10] != null)
            Network.Destroy(Thermometer[10]);

        Thermometer[10] = (GameObject)Network.Instantiate(ThermoT10_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[10].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-1.202358f, -2, 4));

        if (Thermometer[11] != null)
            Network.Destroy(Thermometer[11]);

        Thermometer[11] = (GameObject)Network.Instantiate(ThermoContainer_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[11].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.1488212f, -2, 4));


    }

    public void ThermometerCreationOnPlayerConnected()
    {

        if (Thermometer[0] != null)
            Network.Destroy(Thermometer[0]);
        if (TemperatureDegree == 0)
        {
            Thermometer[0] = (GameObject)Network.Instantiate(ThermoT0_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[0].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(1.5f, -2, 4));
        }

        if (Thermometer[1] != null)
            Network.Destroy(Thermometer[1]);
        if (TemperatureDegree < 2)
        {
            Thermometer[1] = (GameObject)Network.Instantiate(ThermoT1_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[1].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(1.229764f, -2, 4));
        }

        if (Thermometer[2] != null)
            Network.Destroy(Thermometer[2]);
        if (TemperatureDegree < 3)
        {
            Thermometer[2] = (GameObject)Network.Instantiate(ThermoT2_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[2].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.9595285f, -2, 4));
        }

        if (Thermometer[3] != null)
            Network.Destroy(Thermometer[3]);
        if (TemperatureDegree < 4)
        {
            Thermometer[3] = (GameObject)Network.Instantiate(ThermoT3_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[3].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.6892928f, -2, 4));
        }

        if (Thermometer[4] != null)
            Network.Destroy(Thermometer[4]);
        if (TemperatureDegree < 5)
        {
            Thermometer[4] = (GameObject)Network.Instantiate(ThermoT4_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[4].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.419057f, -2, 4));
        }

        if (Thermometer[5] != null)
            Network.Destroy(Thermometer[5]);
        if (TemperatureDegree < 6)
        {
            Thermometer[5] = (GameObject)Network.Instantiate(ThermoT5_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[5].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.1488212f, -2, 4));
        }

        if (Thermometer[6] != null)
            Network.Destroy(Thermometer[6]);
        if (TemperatureDegree < 7)
        {
            Thermometer[6] = (GameObject)Network.Instantiate(ThermoT6_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[6].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.1214144f, -2, 4));
        }

        if (Thermometer[7] != null)
            Network.Destroy(Thermometer[7]);
        if (TemperatureDegree < 8)
        {
            Thermometer[7] = (GameObject)Network.Instantiate(ThermoT7_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[7].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.3916502f, -2, 4));
        }

        if (Thermometer[8] != null)
            Network.Destroy(Thermometer[8]);
        if (TemperatureDegree < 9)
        {
            Thermometer[8] = (GameObject)Network.Instantiate(ThermoT8_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[8].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.661886f, -2, 4));
        }

        if (Thermometer[9] != null)
            Network.Destroy(Thermometer[9]);
        if (TemperatureDegree < 10)
        {
            Thermometer[9] = (GameObject)Network.Instantiate(ThermoT9_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[9].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-0.9321218f, -2, 4));
        }

        if (Thermometer[10] != null)
            Network.Destroy(Thermometer[10]);
        if (TemperatureDegree < 11)
        {
            Thermometer[10] = (GameObject)Network.Instantiate(ThermoT10_Prefab, Vector3.zero, Quaternion.identity, 0);
            Thermometer[10].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-1.202358f, -2, 4));
        }

        if (Thermometer[11] != null)
            Network.Destroy(Thermometer[11]);

        Thermometer[11] = (GameObject)Network.Instantiate(ThermoContainer_Prefab, Vector3.zero, Quaternion.identity, 0);
        Thermometer[11].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0.1488212f, -2, 4));
    }

    public void LaserCreation()
    {
        if (LaserBox != null)
            Network.Destroy(LaserBox);
        LaserBox = (GameObject)Network.Instantiate(LaserBoxPrefab, Vector3.zero, Quaternion.identity, 0);
        LaserBox.GetComponent<GazeTarget>().SetPulseRotate(new Vector3(90, -90, 0));
        LaserBox.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-15, 0.7f, 9));

        if (Halo != null)
            Network.Destroy(Halo);
        Halo = (GameObject)Network.Instantiate(HaloPrefab, Vector3.zero, Quaternion.identity, 0);
        Halo.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-15, 0.1f, 9.0f));
        Halo.GetComponent<GazeTarget>().SetActive(false);

        if (LaserLine != null)
            Network.Destroy(LaserLine);
        LaserLine = (GameObject)Network.Instantiate(LaserLinePrefab, Vector3.zero, Quaternion.identity, 0);
        LaserLine.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0, 0.0f, 9.0f));
    }

    public void DiamondCreation()
    {
        if (Diamond != null)
            Network.Destroy(Diamond);
        Diamond = (GameObject)Network.Instantiate(DiamondPrefab, Vector3.zero, Quaternion.identity, 0);
        Diamond.GetComponent<GazeTarget>().SetPulseRotate(new Vector3(-90, 0, 0));
        Diamond.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(15, 1, 10));
        Diamond.GetComponent<GazeTarget>().SetDiamondColor(Temperature);
        //Diamond.GetComponent<GazeTarget>().SetDiamondTemperature(Temperature);
        //Diamond.GetComponent<MeshRenderer>().material.color = new Color(1.0f, Temperature, Temperature);

    }


    public void PulseCreation()
    {
        if (nbrHitGreenPulse > MaxnbrHitGreenPulse)
        {
            PulseBackwardCreationPhone();
            nbrHitGreenPulse = 0;
        }

        if (ReachPosition)
        {
            if ((nbr_S >= rnd_S) && (nbr_L >= rnd_L))
                LaunchPulse();

            if (nbr_S < rnd_S)
            {
                PulseShortCreationPhone();
            }
            else
                if (nbr_L < rnd_L)
            {
                PulseLongCreationPhone();
            }
        }
    }


    public void PulseMovement()
    {
        float PositionX;

        if (LaunchPoint >= PulseQueueSize)
            LaunchPoint = 0;

        if (LaunchPointBackward >= BackwardPulses.Length)
            LaunchPointBackward = 0;

        for (int i = 0; i < BackwardPulses.Length; i++)
        {
            if (BackwardPulses[i] != null)
            {
                PositionX = BackwardPulses[i].transform.position.x - Speed;
                if (PositionX > StartPositionX)
                {
                    BackwardPulses[i].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(PositionX, 0.0f, 10.0f));
                }
                else
                {
                    UpdateTemperature();

                    HitLaserBox = true;
                    audioSource.PlayOneShot(AlarmClip);

                    for (int j = 0; j < BackwardPulsesPositions.Length; j++)
                    {
                        if (BackwardPulsesPositions[j] == i)
                            BackwardPulsesPositions[j] = -1;
                    }

                    Network.Destroy(BackwardPulses[i]);

                }
            }
            else 
                for (int j = 0; j < BackwardPulsesPositions.Length; j++)
                {
                    if (BackwardPulsesPositions[j] == i)
                    {
                        audioSource.PlayOneShot(DeletePulseClip);
                        score++; BackwardPulsesPositions[j] = -1;
                    }
                }
        }

        for (int i = 0; i < PulseQueueSize; i++)
        {
            if (PulseQueue[i].Pulse != null)
            {

                PositionX = PulseQueue[i].Pulse.transform.position.x + Speed;

                if ((PositionX < EndPosition) && (PositionX > StartPositionX))
                {
                    PulseQueue[i].Pulse.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(PositionX, 0.0f, 10.0f));
                }
                else
                {
                    UpdateTemperature(i);

                    if (PulseQueue[i].Gender == PULSE_GENDER.SHORT)
                        nbrHitGreenPulse++;
                    else
                        for (int j = 0; j < LongPulsesPositions.Length; j++)
                        {
                            if (LongPulsesPositions[j] == i)
                                LongPulsesPositions[j] = -1;
                        }
                    Network.Destroy(PulseQueue[i].Pulse);
                }
            }
            else
            {               
                for (int j = 0; j < LongPulsesPositions.Length; j++)
                {
                    if (LongPulsesPositions[j] == i)
                    {
                        audioSource.PlayOneShot(DeletePulseClip);
                        score++; LongPulsesPositions[j] = -1;
                    }
                }
            }

        }

        if ((CurrentPulse == PULSE_GENDER.SHORT))
            ReachOffSet = ReachOffSet + Speed;

        if (ReachOffSet >= offSet)
        {
            ReachOffSet = 0.0f;
            ReachPosition = true;
        }

        if ((CurrentPulse == PULSE_GENDER.LONG))
            ReachOffSet = ReachOffSet + Speed;

        if ((ReachOffSet >= offSet))
        {
            ReachOffSet = 0.0f;
            ReachPosition = true;
        }

    }

    public void UpdateSpeed()
    {
        if(Speed < MaxSpeed)
            Speed += 0.01f;
    }

    public void UpdateTemperature()
    {
        int gender = 2;
        //thermometerManager.TemperatureBackwardPulseHit();
        Network.Destroy(Thermometer[TemperatureDegree]);
        TemperatureDegree++;
        Temperature = Temperature - 0.09f;
        //Diamond.GetComponent<MeshRenderer>().material.color = new Color(1.0f, Temperature, Temperature);
        Diamond.GetComponent<GazeTarget>().SetDiamondColor(Temperature);
        GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        if (nws != null)
        {
            nws.GetComponent<NetworkSync>().UpdateTemperatureTablet(Temperature, TemperatureDegree);
        }
    }

    public void UpdateTemperature(int i)
    {
        int gender = 0;

        if ((PulseQueue[i].Gender == PULSE_GENDER.LONG) && (PulseQueue[i].Pulse.active))
        {
            gender = 1;
            Network.Destroy(Thermometer[TemperatureDegree]);
            TemperatureDegree++;
            Temperature = Temperature - 0.09f; //0.045f;
            //Diamond.GetComponent<MeshRenderer>().material.color = new Color(1.0f, Temperature, Temperature);
            Diamond.GetComponent<GazeTarget>().SetDiamondColor(Temperature);
            audioSource.PlayOneShot(PulseHitLaserClip);
        }

        GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        if (nws != null)
        {
            nws.GetComponent<NetworkSync>().UpdateTemperatureTablet(Temperature, TemperatureDegree);
        }

    }

    public void UpdateTemperatureTablet(float Temperature, int TemperatureDegreePhone)
    {
        TemperatureDegree = TemperatureDegreePhone;

        //Diamond.GetComponent<MeshRenderer>().material.color = new Color(1.0f, Temperature, Temperature);

        //if (gender == 1)
        //{
        //    thermometerManager.TemperatureLongPulseHitTablet(TemperatureDegree);
        //    Diamond.GetComponent<MeshRenderer>().material.color = new Color(1.0f, Temperature, Temperature);
        //}
        //else if (gender == 2)
        //{
        //    thermometerManager.TemperatureBackwardPulseHitTablet(TemperatureDegree);
        //    Diamond.GetComponent<MeshRenderer>().material.color = new Color(1.0f, Temperature, Temperature);
        //}
    }

    public void OnPlayerConnected()
    {
        if ((VraSettings.instance.isHeadset) && (!GameOverStatus))
        {
            //if (gameOverMessage != null)
               // Network.Destroy(gameOverMessage);

            LaserCreation();
            DiamondCreation();
            ThermometerCreationOnPlayerConnected();
        }

    }

    public void PulseShortCreationPhone()
    {

        PulseQueue[LaunchPoint].Pulse = (GameObject)Network.Instantiate(ShortLaserPrefab, Vector3.zero, Quaternion.identity, 0);
        PulseQueue[LaunchPoint].Pulse.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(StartPositionX, 0.0f, 10.0f));

        LaunchPoint++;
        ReachPosition = false;
        nbr_S++;
        PulseCreated = true;
        CurrentPulse = PULSE_GENDER.SHORT;
        offSet = offSetShort;
    }

    public void PulseLongCreationPhone()
    {

        PulseQueue[LaunchPoint].Pulse = (GameObject)Network.Instantiate(LongLaserPrefab, Vector3.zero, Quaternion.identity, 0);
        PulseQueue[LaunchPoint].Pulse.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(StartPositionX, 0.0f, 10.0f));
        PulseQueue[LaunchPoint].Gender = PULSE_GENDER.LONG;
        PulseQueue[LaunchPoint].Pulse.GetComponent<GazeTargetActionDestroy>().Gender = 1;
        PulseQueue[LaunchPoint].Pulse.GetComponent<GazeTargetActionDestroy>().PulseID = LaunchPoint;

        if (LongPulsePos >= 50)
            LongPulsePos = 0;

        LongPulsesPositions[LongPulsePos] = LaunchPoint;
        LongPulsePos++;

        LaunchPoint++;
        ReachPosition = false;
        nbr_L++;
        PulseCreated = true;
        CurrentPulse = PULSE_GENDER.LONG;
        offSet = offSetLong;

    }

    public void PulseBackwardCreationPhone()
    {
        BackwardPulses[LaunchPointBackward] = (GameObject)Network.Instantiate(BackwardLaserPrefab, Vector3.zero, Quaternion.identity, 0);
        BackwardPulses[LaunchPointBackward].GetComponent<GazeTarget>().SetPulsePosition(new Vector3(EndPosition, 0.0f, 10.0f));
        BackwardPulses[LaunchPointBackward].GetComponent<GazeTargetActionDestroy>().Gender = 2;
        BackwardPulses[LaunchPointBackward].GetComponent<GazeTargetActionDestroy>().PulseID = LaunchPointBackward;

        if (BackwardPulsePos >= 5)
            BackwardPulsePos = 0;

        BackwardPulsesPositions[BackwardPulsePos] = LaunchPointBackward;
        BackwardPulsePos++;

        LaunchPointBackward++;
        PulseCreated = true;
    }


    public void LaunchPulse()
    {
        System.Random rnd = new System.Random();
        rnd_S =  rnd.Next(6, 15);
        nbr_S = 0;
        rnd_L = rnd.Next(1, 3);
        nbr_L = 0;
        PulseCreated = false;

        GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        if (nws != null)
        {
            nws.GetComponent<NetworkSync>().LaunchPulseTablet(rnd_S, nbr_L);
        }

    }

    public void LaunchPulseTablet(int rnd_SPhone, int rnd_LPhone)
    {
        rnd_S = rnd_SPhone; rnd_L = rnd_LPhone;
        nbr_S = 0; nbr_L = 0;

    }

    public void GameOver()
    {
        DestroyGameObject();
        GameOverStatus = true;

        if (gameOverMessage != null)
            Network.Destroy(gameOverMessage);

        gameOverMessage = (GameObject)Network.Instantiate(gameOverMessagePrefab, Vector3.zero, Quaternion.identity, 0);
        gameOverMessage.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(0, 0, 2));
        //gameOverMessage.GetComponent<GameOverMessage>().DisplayGameOverMessage(score);
        gameOverMessage.GetComponent<GameOverMessage>().DisplayGameOverMessage(score) ;
        retryTarget.GetComponent<GazeTargetActionRetryGame>().GameOverMessage = gameOverMessage;
        //gameOverMessage.DisplayGameOverMessage(score);
    }

    public void ShowHighScores()
    {
        this.highScoreKeeper.ShowHighScoresPhone();

        GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");
        if (nws != null)
        {
            nws.GetComponent<NetworkSync>().ShowHighScoresTablet();
        }
    }

    public void ShowHighScoresTablet()
    {
        this.highScoreKeeper.ShowHighScoresTablet();
    }


    public void DestroyGameObject()
    {
        for (int i = 0; i < PulseQueueSize; i++)
            if (PulseQueue[i].Pulse != null)
                Network.Destroy(PulseQueue[i].Pulse);

        for (int i = 0; i < BackwardPulses.Length; i++)
            if (BackwardPulses[i] != null)
                Network.Destroy(BackwardPulses[i]);

        if (Diamond != null)
            Network.Destroy(Diamond);
        if (LaserBox != null)
            Network.Destroy(LaserBox);
        if (LaserLine != null)
            Network.Destroy(LaserLine);
        if (Halo != null)
            Network.Destroy(Halo);

        for (int i = 0; i < 12; i++)
        {
            if (Thermometer[i] != null)
                Network.Destroy(Thermometer[i]);
        }

    }

    public void UpdateScoreTablet(int scorePhone)
    {
        score = scorePhone;
        highScoreKeeper.NewHighScore(score);
    }

    public void Alarm()
    {
        //GameObject nws = GameObject.FindGameObjectWithTag("NetworkSync");

        if (HitLaserBox)
        {
            if (HaloActive)
            {
                if (Halo != null)
                    Network.Destroy(Halo);
                Halo = (GameObject)Network.Instantiate(HaloPrefab, Vector3.zero, Quaternion.identity, 0);
                Halo.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-15, 0.1f, 9.0f));
                Halo.GetComponent<GazeTarget>().SetActive(false);
                //Halo.SetActive(false);
                //if(VraSettings.instance.isGearVR)
                //   Halo.GetComponent<GazeTarget>().SetActive(false);
                //LaserBox.transform.GetChild(0).gameObject.GetComponent<GazeTarget>().SetActive(false);
                //LaserBox.transform.GetChild(0).gameObject.SetActive(false);
                HaloActive = false;
                ////if (nws != null)
                ////{
                ////    nws.GetComponent<NetworkSync>().AlarmTablet(false);
                ////}
                //if (isMine && nw != null)
                //{
                //    Network.RemoveRPCs(nw.viewID);
                //    nw.RPC("AlarmTabletRemote", RPCMode.Others, false);
                //}
            }
            else
            {
                if (Halo != null)
                    Network.Destroy(Halo);
                Halo = (GameObject)Network.Instantiate(HaloPrefab, Vector3.zero, Quaternion.identity, 0);
                Halo.GetComponent<GazeTarget>().SetPulsePosition(new Vector3(-15, 0.1f, 9.0f));
                Halo.GetComponent<GazeTarget>().SetActive(true);
                //if (VraSettings.instance.isGearVR)
                //   Halo.GetComponent<GazeTarget>().SetActive(true);
                //LaserBox.transform.GetChild(0).gameObject.SetActive(true);
                //LaserBox.transform.GetChild(0).gameObject.GetComponent<GazeTarget>().SetActive(true);
                HaloActive = true;
                ////if (nws != null)
                ////{
                ////    nws.GetComponent<NetworkSync>().AlarmTablet(true);
                ////}
                //if (isMine && nw != null)
                //{
                //    Network.RemoveRPCs(nw.viewID);
                //    nw.RPC("AlarmTabletRemote", RPCMode.Others, true);
                //}
            }

            nbrSignal++;

            if (nbrSignal > 5)
            {
                HitLaserBox = false;
                nbrSignal = 0;
            }
        }
    }

    [RPC]
    public void AlarmTabletRemote(bool Alarm )
    {
        LaserBox.transform.GetChild(0).gameObject.SetActive(Alarm);
    }

    public void AlarmTablet(bool Alarm)
    {
        LaserBox.transform.GetChild(0).gameObject.SetActive(Alarm);
    }

}
