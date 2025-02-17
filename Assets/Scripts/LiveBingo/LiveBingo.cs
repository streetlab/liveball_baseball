﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LiveBingo : MonoBehaviour {

	public GameObject mItemBingo;
	public GameObject mItemBingoList;
	Vector3 StartPos = new Vector3(-220f, 220f);
	const float WidthFixed = 146.6f;
	const int RowFixed = 4;
	GetBingoEvent mBingoEvent;
	public GetBingoResponse mBingoResponse;
	GetCurrentLineupEvent mLineupEvent;
	public GetCurrentLineupResponse mLineupResponse;
	CallBingoEvent mCallEvent;
	public CallBingoResponse mCallResponse;
	CallBingoEvent mPowerEvent;
	GameResultEvent mResultEvent;
	int mBingoId;
	bool IsReload;
	bool BoardOnly;
	int mCanGet;
	bool IsNotReady;
	bool IsEnded;
	public int mMsgCount;
	public int mOutCount;

	public Dictionary<int, ItemBingo> mItemDic;
	List<PlayerInfo> mSortedLineup;
	PlayerInfo mPitcher;

	public bool IsMouseDown;

	void OnMouseDown(){
		IsMouseDown = true;
	}

	void OnMouseUp(){
		IsMouseDown = false;
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	public int mUpdateCnt;
	void Update () {
		if(mUpdateCnt++ >= 600){
			mUpdateCnt = 0;
			NetMgr.Alive();
//			NetMgr.JoinGame();
			string msg = "\n[" + UtilMgr.GetDateTimeNow("HH:mm:ss") + "][00ff00]Send:Alive[-],"+mMsgCount;
			transform.FindChild("Top").FindChild("BtnDebug").GetComponent<BtnDebugLiveBingo>().AddLog(msg);
		}
//		NetMgr.UpdateSocket();

		Transform outs = transform.FindChild("Body").FindChild("Scroll View").FindChild("Btm").FindChild("Info")
			.FindChild("BG").FindChild("Outs");

		for(int i = 1; i <= 3; i++){
			outs.FindChild(i+"").FindChild("Red").gameObject.SetActive(false);
			outs.FindChild(i+"").FindChild("Gray").gameObject.SetActive(true);
        }
		for(int i = 1; i <= mOutCount; i++){
			outs.FindChild(i+"").FindChild("Red").gameObject.SetActive(true);
			outs.FindChild(i+"").FindChild("Gray").gameObject.SetActive(false);
		}

	}

	public void Init(){
		mCanGet = 0;
		if(UtilMgr.GetLastBackState() != UtilMgr.STATE.LiveBingo){
			transform.root.FindChild("LiveBingo").gameObject.SetActive(true);
			transform.root.FindChild("LiveBingo").localPosition = new Vector3(2000f, 0);
			NetMgr.JoinGame();
			mUpdateCnt = 0;
			mBingoResponse = null;
			mLineupResponse = null;
			mCallResponse = null;
		}

		IsReload = false;
		BoardOnly = false;
		IsNotReady = false;
		IsEnded = UserMgr.eventJoined.status.Equals("Final") ? true : false;

		ClearBoard();
		mItemDic = new Dictionary<int, ItemBingo>();
		transform.GetComponent<LiveBingoAnimation>().Init();
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("Sprite")
			.FindChild("NotReady").gameObject.SetActive(false);

		mBingoEvent = new GetBingoEvent(ReceivedBingo);
		NetMgr.GetBingo(UserMgr.eventJoined.gameId, mBingoEvent);
	}

	void ReceivedBingo(){
		if(mBingoEvent.Response == null) return;
		mBingoResponse = mBingoEvent.Response;
		mMsgCount = mBingoResponse.data.bingo.msgCount;

		InitBingoBoard();
		if(IsNotReady) return;

		InitBingoBtn();
		if(IsEnded){
			DisableResetBtn();
		} else{
			InitResetBtn();
		}

		if(BoardOnly){
			BoardOnly = false;
			return;
		}

		mLineupEvent = new GetCurrentLineupEvent(ReceivedLineup);
		NetMgr.GetCurrentLineup(UserMgr.eventJoined.gameId, UserMgr.eventJoined.inning,
		                        mBingoResponse.data.bingo.bingoId, mLineupEvent);
	}

	void ReceivedLineup(){
		if(mLineupEvent.Response == null) return;
		mLineupResponse = mLineupEvent.Response;
		mMsgCount = mLineupResponse.data.msgCount;

		mLineupResponse.data.forecast.Sort(
			delegate(CurrentLineupInfo.ForecastInfo x, CurrentLineupInfo.ForecastInfo y) {
			return x.inningNumber.CompareTo(y.inningNumber);
		});

		InitTop();
		InitBtm();
		ShowNext();
	}

	public void BingoClick(){
		mCallEvent = new CallBingoEvent(ReceivedCall);
		NetMgr.CallBingo(UserMgr.eventJoined.gameId, mBingoResponse.data.bingo.bingoId
		                 ,mLineupResponse.data.inningNumber, mCallEvent);
	}

	void ReceivedCall(){
		if(mCallEvent.Response == null) return;
		mCallResponse = mCallEvent.Response;
		if(mCallResponse.data.userRewardGold > 0){
			UserMgr.UserInfo.gold += mCallResponse.data.userRewardGold;
			StartCoroutine(ShowBingoResult());
		} else
			DialogueMgr.ShowDialogue(UtilMgr.GetLocalText("LblBingo"),
			                         string.Format(UtilMgr.GetLocalText("StrFailBingo")
			              , mCallResponse.data.totalRewarded
			              , mCallResponse.data.rewardCount)
			                         , DialogueMgr.DIALOGUE_TYPE.Alert, null);

		ReloadBoard();
	}

	IEnumerator ShowBingoResult(){
		transform.FindChild("PtcBingo").gameObject.SetActive(true);
		transform.FindChild("PtcBingo").GetComponent<ParticleSystem>().Play ();

		yield return new WaitForSeconds(2f);

		DialogueMgr.ShowDialogue(UtilMgr.GetLocalText("LblBingo"),
		                         string.Format(UtilMgr.GetLocalText("StrRewardBingo")
		              , mCallResponse.data.userRewardGold
		              , mCallResponse.data.totalRewarded
		              , mCallResponse.data.rewardCount)
		                         , DialogueMgr.DIALOGUE_TYPE.Alert, BingoDialogue);
	}

	void BingoDialogue(DialogueMgr.BTNS btn){
		if(mCallResponse.data.userBlackBingoReward > 0){
			UserMgr.UserInfo.gold += mCallResponse.data.userBlackBingoReward;
			DialogueMgr.ShowDialogue(UtilMgr.GetLocalText("LblBingo"),
			                         string.Format(UtilMgr.GetLocalText("StrRewardBlackBingo")
			              , mCallResponse.data.userBlackBingoReward)
			                         , DialogueMgr.DIALOGUE_TYPE.Alert, null);
		}
	}

//	int tmp;
	public void ResetClick(){
		DialogueMgr.ShowDialogue(UtilMgr.GetLocalText("StrPowerChance1"),
		                         string.Format(UtilMgr.GetLocalText("StrPowerChance2"), mBingoResponse.data.bingo.powerTimePrice,
		              mBingoResponse.data.bingo.maxPowerTime,
		              mBingoResponse.data.bingo.maxPowerTime-mBingoResponse.data.bingo.powerTimeCount)
		                         , DialogueMgr.DIALOGUE_TYPE.YesNo, DialogPower);
//		transform.GetComponent<LiveBingoAnimation>().ShowBlackBingo();
//		DialogueMgr.ShowDialogue("bingo", "bingo", DialogueMgr.DIALOGUE_TYPE.Alert, null);
//		Reload();
//		if(tmp++ > 10)
//			tmp = 1;
//		transform.GetComponent<LiveBingoAnimation>().SetGauge(tmp, true);
	}

	void DialogPower(DialogueMgr.BTNS btn){
		if(btn == DialogueMgr.BTNS.Btn1){
			if(UserMgr.UserInfo.gold < mBingoResponse.data.bingo.powerTimePrice){
				DialogueMgr.ShowDialogue(UtilMgr.GetLocalText("StrPowerChance1"), UtilMgr.GetLocalText("StrNotEnoughGold2")					
				              , DialogueMgr.DIALOGUE_TYPE.Alert, null);
			} else{
				mPowerEvent = new CallBingoEvent(ReceivedPower);
				NetMgr.PowerMax(UserMgr.eventJoined.gameId, mBingoResponse.data.bingo.bingoId, mPowerEvent);
			}
		}
	}

	void ReceivedPower(){
		UserMgr.UserInfo.gold -= mBingoResponse.data.bingo.powerTimePrice;
		ReloadBoard();
	}

	void MarkBingo(){
		if(mItemDic[11].IsCorrected && mItemDic[21].IsCorrected && mItemDic[31].IsCorrected && mItemDic[41].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("11to41");
		if(mItemDic[12].IsCorrected && mItemDic[22].IsCorrected && mItemDic[32].IsCorrected && mItemDic[42].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("12to42");
		if(mItemDic[13].IsCorrected && mItemDic[23].IsCorrected && mItemDic[33].IsCorrected && mItemDic[43].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("13to43");
		if(mItemDic[14].IsCorrected && mItemDic[24].IsCorrected && mItemDic[34].IsCorrected && mItemDic[44].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("14to44");
		if(mItemDic[11].IsCorrected && mItemDic[12].IsCorrected && mItemDic[13].IsCorrected && mItemDic[14].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("11to14");
		if(mItemDic[21].IsCorrected && mItemDic[22].IsCorrected && mItemDic[23].IsCorrected && mItemDic[24].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("21to24");
		if(mItemDic[31].IsCorrected && mItemDic[32].IsCorrected && mItemDic[33].IsCorrected && mItemDic[34].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("31to34");
		if(mItemDic[41].IsCorrected && mItemDic[42].IsCorrected && mItemDic[43].IsCorrected && mItemDic[44].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("41to44");
		if(mItemDic[11].IsCorrected && mItemDic[22].IsCorrected && mItemDic[33].IsCorrected && mItemDic[44].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("11to44");
		if(mItemDic[14].IsCorrected && mItemDic[23].IsCorrected && mItemDic[32].IsCorrected && mItemDic[41].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().MarkBingo("14to41");

		transform.GetComponent<LiveBingoAnimation>().MarkBlackBingo();
	}

	public void CheckBingo(){
		bool isBlack = true;
		
		foreach(ItemBingo item in mItemDic.Values){
			if(!item.IsCorrected){
				isBlack = false;
				break;
			}
		}
		
		if(isBlack){
			transform.GetComponent<LiveBingoAnimation>().ShowBlackBingo();
			return;
		}
		
		if(mItemDic[11].IsCorrected && mItemDic[21].IsCorrected && mItemDic[31].IsCorrected && mItemDic[41].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("11to41");
		if(mItemDic[12].IsCorrected && mItemDic[22].IsCorrected && mItemDic[32].IsCorrected && mItemDic[42].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("12to42");
		if(mItemDic[13].IsCorrected && mItemDic[23].IsCorrected && mItemDic[33].IsCorrected && mItemDic[43].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("13to43");
		if(mItemDic[14].IsCorrected && mItemDic[24].IsCorrected && mItemDic[34].IsCorrected && mItemDic[44].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("14to44");
		if(mItemDic[11].IsCorrected && mItemDic[12].IsCorrected && mItemDic[13].IsCorrected && mItemDic[14].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("11to14");
		if(mItemDic[21].IsCorrected && mItemDic[22].IsCorrected && mItemDic[23].IsCorrected && mItemDic[24].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("21to24");
		if(mItemDic[31].IsCorrected && mItemDic[32].IsCorrected && mItemDic[33].IsCorrected && mItemDic[34].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("31to34");
		if(mItemDic[41].IsCorrected && mItemDic[42].IsCorrected && mItemDic[43].IsCorrected && mItemDic[44].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("41to44");
		if(mItemDic[11].IsCorrected && mItemDic[22].IsCorrected && mItemDic[33].IsCorrected && mItemDic[44].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("11to44");
		if(mItemDic[14].IsCorrected && mItemDic[23].IsCorrected && mItemDic[32].IsCorrected && mItemDic[41].IsCorrected)
			transform.GetComponent<LiveBingoAnimation>().ShowBingoAni("14to41");
	}

	void InitBingoBoard(){
//		if(mBingoEvent == null || mBingoEvent.Response == null)
//			return;

		if(mBingoResponse.data.bingo == null
		   || mBingoResponse.data.bingo.bingoId == 0){
			SetNotReady();
			return;
		}
		mCanGet = mBingoResponse.data.bingo.bingos - mBingoResponse.data.bingo.rewardedCount;
		
		if(mBingoResponse.data.bingoBoard.Count < 16){
			DialogueMgr.ShowDialogue("Error", "Bingo has less than 16 tiles", DialogueMgr.DIALOGUE_TYPE.Alert, null);
			return;
		}
		
		mBingoId = mBingoResponse.data.bingo.bingoId;

		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board")
			.FindChild("Items").gameObject.SetActive(true);
		int row = 0, col = -1;
		for(int i = 0; i < 16; i++){
			//row col swapped
			if(i % 4 == 0){
				col++; row = -1;
			}
			row++;
			
			if(IsReload){
				ItemBingo tile = transform.FindChild("Body").FindChild("Scroll View").FindChild("Board")
					.FindChild("Items").GetChild(i).GetComponent<ItemBingo>();
				tile.mNewBingoBoard = mBingoResponse.data.bingoBoard[i];
				tile.Init(IsReload);
				continue;
			}
			
			GameObject item = Instantiate(mItemBingo);
			item.transform.parent = transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("Items");
			item.transform.localScale = new Vector3(1f, 1f, 1f);
			item.name = mBingoResponse.data.bingoBoard[i].tailId+"";
			
			item.transform.localPosition = new Vector3(StartPos.x + (WidthFixed * col), StartPos.y - (WidthFixed * row));
			
			ItemBingo itemBingo = item.GetComponent<ItemBingo>();
			itemBingo.mNewBingoBoard = mBingoResponse.data.bingoBoard[i];
			itemBingo.Init(IsReload);
			
			//			if(!IsReload)
			mItemDic.Add(mBingoResponse.data.bingoBoard[i].tailId, item.GetComponent<ItemBingo>());
		}

		transform.GetComponent<LiveBingoAnimation>().SetGauge(mBingoResponse.data.bingo.powerGauge, IsReload);
		if(!IsReload)
			MarkBingo();
	}

	void InitTop(){
		Transform score = transform.FindChild("Top").FindChild("Score");
		score.FindChild("AwayScore").GetComponent<UILabel>().text = mLineupResponse.data.awayTeamRuns+"";
		score.FindChild("HomeScore").GetComponent<UILabel>().text = mLineupResponse.data.homeTeamRuns+"";
		score.FindChild("AwayName").GetComponent<UILabel>().text = UserMgr.eventJoined.awayTeam;
		score.FindChild("HomeName").GetComponent<UILabel>().text = UserMgr.eventJoined.homeTeam;
		if(mLineupResponse.data.inningHalf.Equals("T")){
			score.FindChild("AwayName").FindChild("Sprite").gameObject.SetActive(true);
			score.FindChild("HomeName").FindChild("Sprite").gameObject.SetActive(false);
			int width = score.FindChild("AwayName").GetComponent<UILabel>().width;
			score.FindChild("AwayName").FindChild("Sprite").localPosition = new Vector3(-(width+20), 0, 0);
		} else{
			score.FindChild("AwayName").FindChild("Sprite").gameObject.SetActive(false);
			score.FindChild("HomeName").FindChild("Sprite").gameObject.SetActive(true);
			int width = score.FindChild("HomeName").GetComponent<UILabel>().width;
			score.FindChild("HomeName").FindChild("Sprite").localPosition = new Vector3((width+20), 0, 0);
		}
	}

	public void InitBtm(){
		Transform btm = transform.FindChild("Body").FindChild("Scroll View").FindChild("Btm");
		btm.gameObject.SetActive(true);

		mSortedLineup = new List<PlayerInfo>();
		bool foundLast = false;
		bool isAway = false;
		if(mLineupResponse.data.inningHalf.Equals("T")
		   && UserMgr.eventJoined.inningState.Equals("ING"))
			isAway = true;
		if(mLineupResponse.data.inningHalf.Equals("B")
		   && UserMgr.eventJoined.inningState.Equals("END"))
			isAway = true;
//		T END, B ING
//		if((mLineupResponse.data.inningHalf.Equals("T")
//		   && UserMgr.eventJoined.inningState.Equals("ING"))
//		   || (mLineupResponse.data.inningHalf.Equals("B")
//		 && UserMgr.eventJoined.inningState.Equals("END"))){
		Debug.Log("isAway : "+isAway);
		if(isAway){
			mPitcher = mLineupResponse.data.home.pit;
			for(int i = 0; i < mLineupResponse.data.away.hit.Count; i++){
				if(mLineupResponse.data.away.hit[i].currentBatter > 0){
					foundLast = true;
					for(int j = 0; j < mLineupResponse.data.away.hit.Count; j++){
//						if(++i >= mLineupResponse.data.away.hit.Count) i = 0;
						if(i >= mLineupResponse.data.away.hit.Count) i = 0;
						mSortedLineup.Add(mLineupResponse.data.away.hit[i]);
						i++;
					}
					break;
				}
			}
			if(!foundLast){
				for(int j = 0; j < mLineupResponse.data.away.hit.Count; j++){
					if(j == 0)
						mSortedLineup.Add(mLineupResponse.data.away.hit[mLineupResponse.data.away.hit.Count-1]);
					else
						mSortedLineup.Add(mLineupResponse.data.away.hit[j-1]);
				}
			}
		} else{
			mPitcher = mLineupResponse.data.away.pit;
			for(int i = 0; i < mLineupResponse.data.home.hit.Count; i++){
				if(mLineupResponse.data.home.hit[i].currentBatter > 0){
					foundLast = true;
					for(int j = 0; j < mLineupResponse.data.home.hit.Count; j++){
//						if(++i >= mLineupResponse.data.home.hit.Count) i = 0;
						if(i >= mLineupResponse.data.home.hit.Count) i = 0;
						mSortedLineup.Add(mLineupResponse.data.home.hit[i]);
						i++;
					}
					break;
				}
			}
			if(!foundLast){
				for(int j = 0; j < mLineupResponse.data.home.hit.Count; j++){
					if(j == 0)
						mSortedLineup.Add(mLineupResponse.data.home.hit[mLineupResponse.data.away.hit.Count-1]);
					else
						mSortedLineup.Add(mLineupResponse.data.home.hit[j-1]);
				}
			}
		}

		btm.FindChild("Info").FindChild("BG").FindChild("LblRound").gameObject.SetActive(true);
		btm.FindChild("Info").FindChild("BG").FindChild("LblName").gameObject.SetActive(true);
		if(Localization.language.Equals("English")){
			string roundStr = mLineupResponse.data.inningHalf.Equals("T") ? "Top" : "Bot";
			roundStr += " "+mLineupResponse.data.inningNumber + UtilMgr.GetRoundString(mLineupResponse.data.inningNumber);
			btm.FindChild("Info").FindChild("BG").FindChild("LblRound").GetComponent<UILabel>().text = roundStr;
			btm.FindChild("Info").FindChild("BG").FindChild("LblName").GetComponent<UILabel>().text
				= mPitcher.playerName;
		} else{
			string roundStr = mLineupResponse.data.inningNumber
					+ (mLineupResponse.data.inningHalf.Equals("T") ? "회초" : "회말");
			btm.FindChild("Info").FindChild("BG").FindChild("LblRound").GetComponent<UILabel>().text = roundStr;			
			btm.FindChild("Info").FindChild("BG").FindChild("LblName").GetComponent<UILabel>().text
				= mPitcher.korName;
		}

		int width = btm.FindChild("Info").FindChild("BG").FindChild("LblName").GetComponent<UILabel>().width;
		btm.FindChild("Info").FindChild("BG").FindChild("LblName").FindChild("Label").localPosition = new Vector3(width+16f, -3f);
		btm.FindChild("Info").FindChild("BG").FindChild("LblName").FindChild("Label").GetComponent<UILabel>()
			.text = "#" + mPitcher.backNumber + " ERA    " + mPitcher.ERA;
		btm.FindChild("Info").FindChild("SprCircle").FindChild("Hand").FindChild("Label").GetComponent<UILabel>()
			.text = mPitcher.throwHand;

		btm.FindChild("Info").FindChild("SprCircle").FindChild("Photo").FindChild("Panel")
			.FindChild("Texture").GetComponent<UITexture>().mainTexture = UtilMgr.GetTextureDefault();
		btm.FindChild("Info").FindChild("SprCircle").FindChild("Photo").FindChild("Panel")
			.FindChild("Texture").GetComponent<UITexture>().width = 72;
		btm.FindChild("Info").FindChild("SprCircle").FindChild("Photo").FindChild("Panel")
			.FindChild("Texture").GetComponent<UITexture>().height = 90;
				UtilMgr.LoadImage(mPitcher.playerId,
		                  btm.FindChild("Info").FindChild("SprCircle").FindChild("Photo").FindChild("Panel")
		                  .FindChild("Texture").GetComponent<UITexture>());
				
		btm.FindChild("Draggable").GetComponent<UIDraggablePanel2>().RemoveAll();
//		if(IsEnded){
//			ShowGameEnded();
//			return;
//		}
//		UtilMgr.ClearList(btm.FindChild("Draggable"));
		btm.FindChild("Draggable").GetComponent<UIPanel>().clipOffset = new Vector2(0, 50f);
		btm.FindChild("Draggable").localPosition = new Vector3(0, -283f);
		btm.FindChild("Draggable").GetComponent<UIDraggablePanel2>().Init (mSortedLineup.Count, delegate(UIListItem item, int index) {
			Transform button = item.Target.transform.FindChild("Scroll View").FindChild("Button");
			button.FindChild("Photo").FindChild("Hand").FindChild("Label").GetComponent<UILabel>()
				.text = mSortedLineup[index].batHand;
			string avg = mSortedLineup[index].AVG;
			for(int k = mSortedLineup[index].AVG.Length; k < 5; k++)
				avg += "0";
			button.FindChild("Label").GetComponent<UILabel>()
				.text = "#" + mSortedLineup[index].backNumber + " AVG   " + avg;
			
			if(Localization.language.Equals("English")){
				button.FindChild("LblName").GetComponent<UILabel>()
					.text = mSortedLineup[index].playerName;
			} else{
				button.FindChild("LblName").GetComponent<UILabel>()
					.text = mSortedLineup[index].korName;
			}
			button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().mainTexture
				= UtilMgr.GetTextureDefault();
			button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().width = 72;
			button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().height = 90;
			UtilMgr.LoadImage(mSortedLineup[index].playerId,
			                  button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>());

			JoinQuizInfo joinInfo = new JoinQuizInfo();
			joinInfo.gameId = UserMgr.eventJoined.gameId;
			joinInfo.bingoId = mBingoId;

			if(mLineupResponse == null) return;

			joinInfo.inningNumber = mLineupResponse.data.inningNumber;
			joinInfo.inningHalf = mLineupResponse.data.inningHalf;
			joinInfo.battingOrder = mSortedLineup[index].battingOrder;
			joinInfo.playerId = mSortedLineup[index].playerId;
			//
//			joinInfo.checkValue = -1;
//			foreach(CurrentLineupInfo.ForecastInfo forecast in mLineupEvent.Response.data.forecast){
//				if(forecast.battingOrder == joinInfo.battingOrder){
//					joinInfo.checkValue = forecast.myValue;
//					break;
//				}
//			}
			//
			item.Target.GetComponent<ItemBingoList>().Init(joinInfo);

			if(!UserMgr.eventJoined.status.Equals("Scheduled")){
				int lockCnt = 1;
				if(UtilMgr.IsMLB()){
					lockCnt = 2;
				}

				if(index < lockCnt){
					item.Target.GetComponent<ItemBingoList>().SetToLocking();
				}

			}
		});

		if(UserMgr.eventJoined.status.Equals("Scheduled")){
			transform.root.FindChild("LiveBingo").GetComponent<LiveBingoAnimation>().SetItemBlink(-1);
		} else
			transform.root.FindChild("LiveBingo").GetComponent<LiveBingoAnimation>()
				.SetItemBlink(mSortedLineup[0].playerId);


		btm.FindChild("Draggable").GetComponent<UIDraggablePanel2>().ResetPosition();

//		UtilMgr.ClearList(btm.FindChild("Draggable"));
//		float height = 90f;
//		for(int index = 0; index < mSortedLineup.Count; index++){
//			GameObject go = Instantiate(mItemBingoList);
//			go.transform.parent = btm.FindChild("Draggable");
//			go.transform.localScale = new Vector3(1f, 1f, 1f);
//			go.transform.localPosition = new Vector3(0, height);
//			height -= 120f;
//			Transform button = go.transform.FindChild("Scroll View").FindChild("Button");
//			button.FindChild("Photo").FindChild("Hand").FindChild("Label").GetComponent<UILabel>()
//				.text = mSortedLineup[index].batHand;
//			button.FindChild("Label").GetComponent<UILabel>()
//				.text = "#" + mSortedLineup[index].backNumber + " AVG " + mSortedLineup[index].AVG;
//			
//			if(Localization.language.Equals("English")){
//				button.FindChild("LblName").GetComponent<UILabel>()
//					.text = mSortedLineup[index].playerName;
//			} else{
//				button.FindChild("LblName").GetComponent<UILabel>()
//					.text = mSortedLineup[index].korName;
//			}
//			button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().mainTexture
//				= UtilMgr.GetTextureDefault();
//			button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().width = 72;
//			button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().height = 90;
//			UtilMgr.LoadImage(mSortedLineup[index].photoUrl,
//			                  button.FindChild("Photo").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>());
//			
//			JoinQuizInfo joinInfo = new JoinQuizInfo();
//			joinInfo.gameId = UserMgr.eventJoined.gameId;
//			joinInfo.bingoId = mBingoId;
//			joinInfo.inningNumber = mLineupEvent.Response.data.inningNumber;
//			joinInfo.inningHalf = mLineupEvent.Response.data.inningHalf;
//			joinInfo.battingOrder = mSortedLineup[index].battingOrder;
//			joinInfo.playerId = mSortedLineup[index].playerId;
//			go.GetComponent<ItemBingoList>().Init(joinInfo);
//
//			if(index < 2)
//				go.GetComponent<ItemBingoList>().SetToLocking();
//		}
//		btm.FindChild("Draggable").GetComponent<UIScrollView>().ResetPosition();
	}

	void ClearBoard(){
		Transform tf = transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("Items");
		GameObject[] gos = new GameObject[tf.childCount];
		for(int i = 0; i < gos.Length; i++){
			gos[i] = tf.GetChild(i).gameObject;
			gos[i].SetActive(false);
		}
//		tf.DetachChildren();
		for(int i = 0; i < gos.Length; i++){
			Destroy(gos[i]);
			//			DestroyImmediate(gos[i]);
		}
	}

	public void ReceivedResult(SocketMsgInfo info){
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("Result")
			.GetComponent<BingoResult>().SocketResult(info);
		StartCoroutine(IReloadBoard(info));
//		ReloadLineup();
		mOutCount = info.data.outs;
	}

	IEnumerator IReloadBoard(SocketMsgInfo info){
		yield return new WaitForSeconds(3f);
//		ReloadBoard();
		if(info.data.inningState.Equals("END")){
			UserMgr.eventJoined.inningState = "END";
			UserMgr.eventJoined.status = "Scheduled";
			InitBtm();
			ReloadBoard();
		}
		else
			ReloadAll();
	}

	public void ReloadBoard(){
		IsReload = true;
		BoardOnly = true;
		mBingoEvent = new GetBingoEvent(ReceivedBingo);
		NetMgr.GetBingo(UserMgr.eventJoined.gameId, mBingoEvent);
	}

	public void ChangePlayer(SocketMsgInfo info){
		UserMgr.eventJoined.inningState = "ING";
		UserMgr.eventJoined.status = "InProgress";
		
		if(info.data.changeBingo > 0){
			Init ();
			return;
		}

//		try{
//			if(info.data.pitcherId != mPitcher.playerId
//			   || info.data.playerId != mSortedLineup[0].playerId){
//				Debug.Log ("Lineup Changed");
//				throw new UnassignedReferenceException();
//			}
//
//		} catch{
			ReloadLineup (info);
//		}
		mOutCount = info.data.outs;
	}

	public void ReloadLineup(SocketMsgInfo info){
//		UserMgr.eventJoined.inningState = "ING";
//		UserMgr.eventJoined.status = "InProgress";
//
//		if(info.data.changeBingo > 0){
//			Init ();
//			return;
//		}
//		IsReload = true;
//		int inning = mLineupResponse.data.inningNumber;
		int inning = 0;
		UserMgr.eventJoined.inningState = info.data.inningState;
		mLineupEvent = new GetCurrentLineupEvent(ReceivedLineup);
		NetMgr.GetCurrentLineup(UserMgr.eventJoined.gameId, inning,
		                        mBingoId, mLineupEvent);
	}

	public void ReloadAll(){
		IsReload = true;
		mBingoEvent = new GetBingoEvent(ReceivedBingo);
		NetMgr.GetBingo(UserMgr.eventJoined.gameId, mBingoEvent);
	}

	public void ChangeInning(SocketMsgInfo info){
		UserMgr.eventJoined.inningState = info.data.inningState;
		if(info.data.changeBingo > 0){
			Init ();
			return;
		}

		if(mLineupResponse == null)
			return;
		mLineupResponse.data.inningHalf = info.data.inningHalf;
		mLineupResponse.data.inningNumber = int.Parse(info.data.inning);
		if(info.data.inningState.Equals("END"))
			UserMgr.eventJoined.status = "Scheduled";

		Debug.Log("state : "+UserMgr.eventJoined.inningState+",half : "+mLineupResponse.data.inningHalf);
		InitBtm();
//		IsReload = true;
//		mLineupEvent = new GetCurrentLineupEvent(ReceivedLineup);
//		NetMgr.GetCurrentLineup(UserMgr.eventJoined.gameId, int.Parse(info.data.inning),
//		                        mBingoEvent.Response.data.bingo.bingoId, mLineupEvent);
	}

	public void InitBingoBtn(){
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo").FindChild("Sprite")
			.gameObject.SetActive(false);
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo")
			.GetComponent<UIButton>().isEnabled = false;
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo")
			.FindChild("LblBingo").GetComponent<UILabel>().color = new Color(187f/255f, 187f/255f, 187f/255f);
		if(mCanGet > 0){
			transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo")
				.GetComponent<UIButton>().isEnabled = true;
			transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo").FindChild("Sprite")
				.gameObject.SetActive(true);
			transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo").FindChild("Sprite")
				.FindChild("Label").GetComponent<UILabel>().text = mCanGet+"";
			transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo")
				.FindChild("LblBingo").GetComponent<UILabel>().color = Color.white;
		}

//		if(IsReload){
//
//		} else{
//
//		}

	}

	void InitResetBtn(){
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnReset").
			GetComponent<UIButton>().isEnabled = true;
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnReset").FindChild("Sprite2")
			.GetComponent<UISprite>().color = new Color(0, 106f/255f, 216f/255f);
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnReset").FindChild("Background")
			.GetComponent<UISprite>().color = new Color(0, 106f/255f, 216f/255f);
	}

	void DisableResetBtn(){
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnReset").
			GetComponent<UIButton>().isEnabled = false;
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnReset").FindChild("Sprite2")
			.GetComponent<UISprite>().color = new Color(128f/255f, 128f/255f, 128f/255f);
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnReset").FindChild("Background")
			.GetComponent<UISprite>().color = new Color(128f/255f, 128f/255f, 128f/255f);
	}

	void SetNotReady(){
		IsNotReady = true;
//	dateTime: "20160426053000",
		string dateTime = UtilMgr.IsMLB() ? UserMgr.eventJoined.dateTime : UserMgr.eventJoined.korDateTime;
		string timeZone = UtilMgr.IsMLB() ? "ET" : "KST";
		int month = int.Parse(dateTime.Substring(4, 2));
		int date = int.Parse(dateTime.Substring(6, 2));
		int hour = int.Parse(dateTime.Substring(8, 2));
		string min = dateTime.Substring(10, 2);
		string strTime = Localization.language.Equals("English") ?
			UtilMgr.GetMonthString(month) + " " + date + ", " + timeZone + " " 
				+ UtilMgr.GetAMPM(hour)[0] + ":" + min + " " + UtilMgr.GetAMPM(hour)[1] :
				month + "월 " + date +"일, " + timeZone + " "
				+ UtilMgr.GetAMPM(hour)[0] + ":" + min + " " + UtilMgr.GetAMPM(hour)[1] ;

		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("Sprite")
			.FindChild("NotReady").FindChild("Label1").GetComponent<UILabel>().text
				= string.Format(UtilMgr.GetLocalText("StrBingoSub1"), strTime);

		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("Sprite")
			.FindChild("NotReady").FindChild("Label2").GetComponent<UILabel>().text
				= UtilMgr.GetLocalText("StrBingoSub2");

		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("Sprite")
			.FindChild("NotReady").gameObject.SetActive(true);

		Transform score = transform.FindChild("Top").FindChild("Score");
		score.FindChild("AwayScore").GetComponent<UILabel>().text = "0";
		score.FindChild("HomeScore").GetComponent<UILabel>().text = "0";
		score.FindChild("AwayName").GetComponent<UILabel>().text = UserMgr.eventJoined.awayTeam;
		score.FindChild("HomeName").GetComponent<UILabel>().text = UserMgr.eventJoined.homeTeam;
		score.FindChild("AwayName").FindChild("Sprite").gameObject.SetActive(false);
		score.FindChild("HomeName").FindChild("Sprite").gameObject.SetActive(false);

		Transform btm = transform.FindChild("Body").FindChild("Scroll View").FindChild("Btm");

		btm.FindChild("Info").FindChild("BG").FindChild("LblRound").GetComponent<UILabel>()
			.text = "";
		btm.FindChild("Info").FindChild("BG").FindChild("LblName").FindChild("Label").GetComponent<UILabel>()
			.text = "";
		btm.FindChild("Info").FindChild("BG").FindChild("LblName").GetComponent<UILabel>()
			.text = "";
		btm.FindChild("Info").FindChild("SprCircle").FindChild("Hand").FindChild("Label").GetComponent<UILabel>()
			.text = "";

		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo").FindChild("Sprite")
			.gameObject.SetActive(false);
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Board").FindChild("BtnBingo")
			.GetComponent<UIButton>().isEnabled = false;

		DisableResetBtn();

		btm.FindChild("Draggable").GetComponent<UIDraggablePanel2>().RemoveAll();

		ShowNext();
	}

	void ShowNext(){
		if(!IsReload && UtilMgr.GetLastBackState() != UtilMgr.STATE.LiveBingo){
			StartCoroutine(Next ());
		}
		if(IsEnded)
			ShowGameEnded();
	}

	IEnumerator Next(){
		transform.FindChild("Top").gameObject.SetActive(false);
		transform.FindChild("Body").gameObject.SetActive(false);
		yield return new WaitForSeconds(0.1f);
		transform.FindChild("Top").gameObject.SetActive(true);
		transform.FindChild("Body").gameObject.SetActive(true);
		UtilMgr.AddBackState(UtilMgr.STATE.LiveBingo);
		UtilMgr.AnimatePageToLeft("Lobby", "LiveBingo");

//		if(UserMgr.eventJoined.status.Equals("Final")){
//			ShowGameEnded();
//		}
	}

	public void GameEnded(){
		UserMgr.eventJoined.status = "Final";
		Init ();
	}

	public void ShowGameEnded(){
		DialogueMgr.ShowDialogue(UtilMgr.GetLocalText("LblLiveBingo"),
		                         UtilMgr.GetLocalText("StrBingoEnded"), DialogueMgr.DIALOGUE_TYPE.Alert, null);

		mResultEvent = new GameResultEvent(ReceivedGameResult);
		NetMgr.GameResult(mResultEvent);
	}

	void ReceivedGameResult(){
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Btm").gameObject.SetActive(false);
		transform.FindChild("Body").FindChild("Scroll View").FindChild("Result").gameObject.SetActive(true);
		Transform result = transform.FindChild("Body").FindChild("Scroll View").FindChild("Result");
		result.FindChild("Info").FindChild("BG").FindChild("LblAway").GetComponent<UILabel>()
			.text = UserMgr.eventJoined.awayTeam;
		result.FindChild("Info").FindChild("BG").FindChild("LblHome").GetComponent<UILabel>()
			.text = UserMgr.eventJoined.homeTeam;

//		Debug.Log("awayTeamId : "+UserMgr.eventJoined.awayTeamId);
//		Debug.Log("homeTeamId : "+UserMgr.eventJoined.homeTeamId);
//
//		GameResultInfo.TeamResultInfo away;
//		GameResultInfo.TeamResultInfo home;

		result.FindChild("LblHits").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].hits, mResultEvent.Response.data.team[1].hits);
		result.FindChild("LblHomeruns").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].homeRuns, mResultEvent.Response.data.team[1].homeRuns);
		result.FindChild("LblBaseonballs").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].walks, mResultEvent.Response.data.team[1].walks);
		result.FindChild("LblHitbypitches").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].hitByPitch, mResultEvent.Response.data.team[1].hitByPitch);
		result.FindChild("LblStrikeouts").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].strikeouts, mResultEvent.Response.data.team[1].strikeouts);
		result.FindChild("LblStolenBases").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].stolenBases, mResultEvent.Response.data.team[1].stolenBases);
		result.FindChild("LblDoublePlays").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].doublePlay, mResultEvent.Response.data.team[1].doublePlay);
		result.FindChild("LblErrors").GetComponent<LiveResultValue>().Init(
			mResultEvent.Response.data.team[0].err, mResultEvent.Response.data.team[1].err);
	}

	public void OnPitcherClick(){
		PlayerInfo player = null;
		try{
			player = UserMgr.PlayerDic[mPitcher.playerId];
		} catch{
			DialogueMgr.ShowDialogue("Error", "Failed to find the information of that player."
			                         , DialogueMgr.DIALOGUE_TYPE.Alert, null);
		}

		if(player == null) return;
		transform.root.FindChild("PlayerCard").GetComponent<PlayerCard>().Init(player, null);
	}

	public void OnBackClick(){
		if(mCanGet > 0){
			DialogueMgr.ShowDialogue(UtilMgr.GetLocalText("LblLiveBingo"), 
			                         UtilMgr.GetLocalText("StrBingoRemained"), DialogueMgr.DIALOGUE_TYPE.Alert, null);
		} else
			transform.root.GetComponent<SuperRoot>().OnBackPressed();
	}
}
