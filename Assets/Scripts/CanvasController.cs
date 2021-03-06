﻿//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using UnityEngine.EventSystems;
using UnityEditor;
using System.Collections;

public class CanvasController : MonoBehaviour {

	private const string CANVAS_ANIMATOR_STATE = "CanvasState";
	private const int HIDE_INTRO_PANEL = 1;
	private const int FIND_SURFACE_PANEL_ENTER = 2;
	private const int PUT_MAP_PANEL_ENTER = 4;
	private const int TOUCH_PIN_PANEL_ENTER = 6;
	private const int MODEL_TEXT_ENTER = 8;
	private const int FAST_HIDE_PINS = 66;
	private const int ANIM_EXIT = 0;


	private Animator _animator;



	private AnimationComponent component;

	public UnityARGeneratePlane generatePlaneScript;
	public Button ok_intro;
	public Button back_Button;
	public Button reload_Button;
	public Button screenShot_Button;
	public Button info_Button;
	public Button share_Button;


	public GameObject intro_Panel;
	public GameObject about_map_Panel;
	public GameObject about_pins_Panel;
	public GameObject find_surface_Panel;
	public GameObject screenShot_Panel;
	public Text modelName_Text;

	public GameObject mIpadupperPanel;
	public GameObject mIpadbotPanel;



	public UnityARAnchorManager anchManager;


	private bool first_Enter = true;
	private bool clickedFromUI;
	public static bool isFirstSession;
	private int currentCanvasState;

	float exitShareButton_Timer = 0;
	public bool isModelScene = false;
	private bool isCoroutineRunning = false;
	[SerializeField]
	private ShareFun shareObj;


	void Start () {
		isCoroutineRunning = false;
		isModelScene = false;
		currentCanvasState = 0;
		_animator = GetComponent<Animator>();
		component = new AnimationComponent();
		
		hide_back_Button ();
		hide_reload_btn ();
		hide_info_Button();
		hide_screenShot_btn ();
		isFirstSession = SaveManager.Instance.session_state.isFirstEnter;

		if(!isFirstSession){
			show_info_Button();
		} 
		startGeneratePlane();

		bool isX = UnityEngine.iOS.Device.generation == UnityEngine.iOS.DeviceGeneration.iPhoneX;
		if(isX){
			about_map_Panel.transform.position += new Vector3(0,-120f,0);
			about_pins_Panel.transform.position += new Vector3(0,-120f,0);
			find_surface_Panel.transform.position += new Vector3(0,-120f,0);			
		}
	}
	
	public void close_introduction(){
		close_intro ();
	}
	public void open_introduction(){
		show_intro ();
	}
	private void close_intro(){
	setCanvasAnimatorParametr(HIDE_INTRO_PANEL);
	}
	private void show_intro(){
		ok_intro.transform.localScale = new Vector3 (1f, 1f, 1f);
		intro_Panel.SetActive(true);
	}

	public void show_find_surface_info(){
		find_surface_Panel.SetActive(true);
		setCanvasAnimatorParametr(FIND_SURFACE_PANEL_ENTER);
	}
	public void hide_find_surface_info(){
		setCanvasAnimatorParametr(ANIM_EXIT);
		Invoke("EVENT_surface_info_exit",0.4f);
	}

	public void EVENT_surface_info_exit(){
		find_surface_Panel.SetActive(false);
		if (SaveManager.Instance.session_state.isFirstEnter)
			show_about_map_text();
	}

	public void show_about_map_text(){
		about_map_Panel.SetActive (true);
		setCanvasAnimatorParametr(PUT_MAP_PANEL_ENTER);
	}
	public void hide_about_map_text(){
		setCanvasAnimatorParametr(ANIM_EXIT);
	}
	public void EVENT_put_map_exit(){
		about_map_Panel.SetActive(false);
		if(SaveManager.Instance.session_state.isFirstEnter)
			show_about_pins();
	}

	public void show_about_pins(){
		about_pins_Panel.SetActive (true);
		setCanvasAnimatorParametr(TOUCH_PIN_PANEL_ENTER);
	}
	public void hide_about_pins(){
		setCanvasAnimatorParametr(ANIM_EXIT);
	}
	public void EVENT_about_pins_exit(){
		about_pins_Panel.SetActive(false);
	}

	public void show_modelName_Text(string text){
		modelName_Text.gameObject.SetActive(true);
		modelName_Text.text = text.ToUpper();
		setCanvasAnimatorParametr(MODEL_TEXT_ENTER);
	}
	public void hide_modelName_Text(){
		setCanvasAnimatorParametr(ANIM_EXIT);
		Invoke("hide_modelName_EVENT",0.5f);
	}
	private void hide_modelName_EVENT(){
		modelName_Text.gameObject.SetActive(false);
	}

	public void show_info_Button(){
		info_Button.gameObject.SetActive(true);
	}

	public void hide_info_Button(){
		info_Button.gameObject.SetActive(false);
	}


	public void show_back_Button(){
		back_Button.transform.localScale = new Vector3 (1f, 1f, 1f);
	}
	public void hide_back_Button(){
		back_Button.transform.localScale = new Vector3 (0, 0, 0);
	}

	public void hide_reload_btn(){
		reload_Button.transform.localScale = new Vector3 (0,0,0);
	}
	public void show_reload_btn(){
		reload_Button.transform.localScale = new Vector3 (1f,1f,1f);
		
	}
	public void hide_screenShot_btn(){
		screenShot_Button.transform.localScale = new Vector3 (0, 0, 0);
	}
	public void show_share_btn(){
		share_Button.GetComponent<Animator>().Play("Share_Anim_Enter");	
	}
	public void hide_share_btn(){
		share_Button.GetComponent<Animator>().Play("Share_Anim_Exit");
		Invoke("close_share",0.5f);
	}
	public void show_screenShot_btn(){
		screenShot_Button.transform.localScale = new Vector3 (1f, 1f, 1f);
	}


	public void screenShot_Flash(){
		screenShot_Panel.SetActive(true);
		screenShot_Panel.GetComponent<Animator>().Play("screenCupture_anim");
		Invoke("screenshot_anim_EVENT",0.5f);
	}
	private void screenshot_anim_EVENT(){
		screenShot_Panel.SetActive(false);
		setUIVisible(true);
		if(SaveManager.Instance.session_state.isFirstEnter)
			hide_about_pins();
		exitShareButton_Timer = 3f;
		Debug.Log("Countfind - isActive " + share_Button.gameObject.activeInHierarchy);
		if(!share_Button.gameObject.activeInHierarchy){
			share_Button.gameObject.SetActive(true);	
			show_share_btn();
			Debug.Log("Countfind Corutine started");

			StartCoroutine(ShareButton_AnimTimer());
			isCoroutineRunning = true;

		} 
	}
	IEnumerator ShareButton_AnimTimer(){
		
		bool waitForCloseButton = true;
		while (waitForCloseButton){
			
			yield return new WaitForSeconds(1f);
			exitShareButton_Timer--;
			if(exitShareButton_Timer <= 0){		
				waitForCloseButton = false;
				
				hide_share_btn();

				if(SaveManager.Instance.session_state.isFirstEnter)
					 show_about_pins();

				isCoroutineRunning = false;
			}
		}
    }
	public void close_share(){
		isCoroutineRunning = false;
		exitShareButton_Timer = 0;
		Color color = share_Button.GetComponent<Color>();
		color.a = 0;
		share_Button.transform.position += new Vector3(256,0,0);
		share_Button.gameObject.SetActive(false);
	}


	void startGeneratePlane(){
		intro_Panel.SetActive(false);
		generatePlaneScript.initStart();
		Invoke("showFirstHelp",0.3f);
	}
	private void showFirstHelp(){
		show_find_surface_info();
	}
	private void setCanvasAnimatorParametr(int transitionState){
		_animator.SetInteger(CANVAS_ANIMATOR_STATE,transitionState);
		currentCanvasState = transitionState;
	}

	public void setUIVisible(bool visible){

		screenShot_Button.gameObject.SetActive(visible);
		info_Button.gameObject.SetActive(visible);
		reload_Button.gameObject.SetActive(visible);
		modelName_Text.gameObject.SetActive(visible);
		back_Button.gameObject.SetActive(visible);

		if(isCoroutineRunning )
			share_Button.gameObject.SetActive(visible);

		if (SaveManager.Instance.session_state.isFirstEnter && !isCoroutineRunning){
			hide_Helper_ToShowShare();
		} else if(!visible)
			uiPreparedForCapture();
	}
	public void uiPreparedForCapture(){
		shareObj.playCoroutine();
	}

	public void resetAnimationState(){
		setCanvasAnimatorParametr(ANIM_EXIT);
	}

	private void hide_Helper_ToShowShare(){
		setCanvasAnimatorParametr(FAST_HIDE_PINS);
	}
}
