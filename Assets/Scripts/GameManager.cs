﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour {

	//定数定義
	private const int MAX_ORB = 10; //オーブ最大数
	private const int RESPAWN_TIME = 5;//オーブが発生する秒数
	private const int MAX_LEVEL = 2;//最大お寺レベル



	//オブジェクト参照
	public GameObject orbPrefab; //オーブプレハブ
	public GameObject smokePrefab;//煙プレハブ
	public GameObject kusudamaPrefab;//くす玉プレハブ
	public GameObject canvasGame; //ゲームキャンパス
	public GameObject textScore; //スコアテキスト
	public GameObject imageTemple;//お寺

	public GameObject imageMokugyo;//木魚

	public AudioClip getScoreSE; //効果音スコアゲット
	public AudioClip levelUPSE;//効果音レベルアップ
	public AudioClip clearSE;//効果音クリア





	//メンバ変数
	private int score = 0;//現在のスコア
	private int nextScore = 10;//レベルアップまでに必要なスコア

	private int currentOrb = 0;//現在のオーブ数

	private int templeLevel =0;//寺のレベル

	private DateTime lastDateTime;//前回オーブを生成した時間

	private int[] nextScoreTable = new int[] { 10, 100, 1000 };//レベルアップ値

	private AudioSource audioSource;//オーディオソース




	//寺のレベル管理
	void TemplelevelUp(){
		if (score >= nextScore) {
			if (templeLevel < MAX_LEVEL) {
				templeLevel++;
				score = 0;

				TempleLevelUpEffect ();

				nextScore = nextScoreTable [templeLevel];
				imageTemple.GetComponent<TempleManager> ().SetTemplePicture (templeLevel);
			}
		}
	}
	//寺が最後まで育った時の演出
	void ClearEffect(){
		GameObject kusudama = (GameObject)Instantiate (kusudamaPrefab);
		kusudama.transform.SetParent (canvasGame.transform, false);

		audioSource.PlayOneShot (clearSE);
	}

	// Use this for initialization
	void Start () {

		//オーディオソース取得
		audioSource = this.gameObject.GetComponent<AudioSource>();

		currentOrb = 10;

		//初期オーブ設定
		for (int i = 0; i < currentOrb; i++) {
			CreateOrb ();
		}
		//初期設定
		lastDateTime = DateTime.UtcNow;

		nextScore = nextScoreTable [templeLevel];
		imageTemple.GetComponent<TempleManager>().SetTemplePicture (templeLevel);
		imageTemple.GetComponent<TempleManager>().SetTempleScale (score, nextScore);
		RefreshScoreText ();

		
	}
	
	// Update is called once per frame
	void Update () {
		
		if(currentOrb < MAX_ORB){
			TimeSpan timeSpan = DateTime.UtcNow - lastDateTime;

			if(timeSpan >= TimeSpan.FromSeconds(RESPAWN_TIME)){
				while (timeSpan >= TimeSpan.FromSeconds (RESPAWN_TIME)) {
					CreateNewOrb ();
					timeSpan -= TimeSpan.FromSeconds (RESPAWN_TIME);
			}
		}
	}

}
	//レベルアップ時の演出
	void TempleLevelUpEffect(){
		GameObject smoke = (GameObject)Instantiate (smokePrefab);
		smoke.transform.SetParent (canvasGame.transform, false);
		smoke.transform.SetSiblingIndex (2);

		audioSource.PlayOneShot (levelUPSE);

		Destroy (smoke, 0.5f);

	}

	//新しいオーブの生成
	public void CreateNewOrb(){
		lastDateTime = DateTime.UtcNow;
		if (currentOrb >= MAX_ORB) {
			return;
		}

		CreateOrb ();
		currentOrb++;

	}

	//オーブ生成
	public void CreateOrb(){
		GameObject Orb = (GameObject)Instantiate (orbPrefab);
		Orb.transform.SetParent (canvasGame.transform, false);
		Orb.transform.localPosition = new Vector3 (
			UnityEngine.Random.Range (-300.0f, 300.0f),
			UnityEngine.Random.Range (-140.0f, -500.0f),
			0f);

	//オーブの種類を設定
	int kind = UnityEngine.Random.Range(0,templeLevel+1);
	switch (kind){
	case 0:
			Orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.BLUE);
	break;
	case 1:
			Orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.GREEN);
	break;
	case 2:
			Orb.GetComponent<OrbManager>().SetKind(OrbManager.ORB_KIND.PURPLE);
	break;
	}
}
	//オーブ入手
	public void GetOrb(int getScore){

		audioSource.PlayOneShot (getScoreSE);
		//木魚アニメ再生
		AnimatorStateInfo stateInfo =
			imageMokugyo.GetComponent<Animator> ().GetCurrentAnimatorStateInfo (0);
		if (stateInfo.fullPathHash ==
		    Animator.StringToHash ("Base Layer.get@ImageMokugyo")) {
			//すでに再生中なら
			imageMokugyo.GetComponent<Animator> ().Play (stateInfo.fullPathHash, 0, 0.0f);
		} else {
			imageMokugyo.GetComponent<Animator> ().SetTrigger ("isGetScore");
		}

		if (score < nextScore) {
			score += getScore;

			//レベルアップ値を超えないよう制限
			if (score > nextScore) {
				score = nextScore;
			}

			TemplelevelUp ();

			RefreshScoreText ();

			imageTemple.GetComponent<TempleManager> ().SetTempleScale (score, nextScore);

			//ゲームクリア判定
			if ((score == nextScore) && (templeLevel == MAX_LEVEL)) {
				ClearEffect ();
			}
		}


		currentOrb--;
	}

	//スコアテキスト更新
	void RefreshScoreText(){
		textScore.GetComponent<Text> ().text = "徳：" + score + "/" + nextScore;
	}

			

}