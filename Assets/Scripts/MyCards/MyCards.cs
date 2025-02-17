﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MyCards : MonoBehaviour {

	GetCardInvenEvent mCardEvent;
	GetMailEvent mMailEvent;

	List<CardInfo> mList;
	
	// Use this for initialization
	void Start () {
			
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public GetMailEvent GetMailEvent(){
		return mMailEvent;
	}

	public void Init(GetCardInvenEvent cardEvent, GetMailEvent mailEvent){
		transform.gameObject.SetActive(true);
		transform.FindChild("Top").FindChild("Cards").FindChild("LblCardsV").GetComponent<UILabel>().text
			= cardEvent.Response.data.Count+" / "+UserMgr.LobbyInfo.userInvenOfCard;

//		transform.FindChild("Top").FindChild("Skills").FindChild("LblSkillsV").GetComponent<UILabel>().text
//			= 0+"";

		mCardEvent = cardEvent;
		mMailEvent = mailEvent;

		mList = cardEvent.Response.data;
		foreach(CardInfo cardInfo in mList)
			cardInfo.mType = CardInfo.INVEN_TYPE.CARD;

		for(int i = 0; i < mailEvent.Response.data.Count; i++){
			Mailinfo mailInfo = mailEvent.Response.data[i];
			if(mailInfo.mailType == 1){
				CardInfo item = new CardInfo();
				item.mType = CardInfo.INVEN_TYPE.PACK;
				item.mMailinfo = mailInfo;
				mList.Insert(0, item);
			}
		}

		int listCnt = mList.Count;

		if(UserMgr.LobbyInfo.userInvenOfCard < UserMgr.LobbyInfo.maxInvenOfCard
		   && cardEvent.Response.data.Count > UserMgr.LobbyInfo.userInvenOfCard){
			CardInfo expand = new CardInfo();
			expand.mType = CardInfo.INVEN_TYPE.EXPAND;
			mList.Insert(UserMgr.LobbyInfo.userInvenOfCard, expand);
			listCnt = UserMgr.LobbyInfo.userInvenOfCard +1;
		}

		transform.FindChild("Body").FindChild("Draggable").GetComponent<UIDraggablePanel2>().RemoveAll();
		transform.FindChild("Body").FindChild("Draggable").GetComponent<UIDraggablePanel2>()
			.Init(listCnt, delegate (UIListItem item, int index){
				InitInvenItem(item, index);
		});
		transform.FindChild("Body").FindChild("Draggable").GetComponent<UIDraggablePanel2>().ResetPosition();
	}

	void InitInvenItem(UIListItem item, int index){
		CardInfo info = mList[index];
		item.Target.GetComponent<ItemInvenCard>().mCardInfo = info;

		if(info.mType == CardInfo.INVEN_TYPE.CARD){
			item.Target.transform.FindChild("ItemCardPack").gameObject.SetActive(false);
			item.Target.transform.FindChild("ItemCard").gameObject.SetActive(true);
			item.Target.transform.FindChild("ItemExpand").gameObject.SetActive(false);

			Transform tf = item.Target.transform.FindChild("ItemCard");

			tf.GetComponent<ItemCard>().mCardInfo = info;
			tf.FindChild("LblPosition").GetComponent<UILabel>().text = info.position;


			if(Localization.language.Equals("English")){
				tf.FindChild("LblName").GetComponent<UILabel>().text = info.firstName + " " + info.lastName;
				if(tf.FindChild("LblName").GetComponent<UILabel>().width > 232)
					tf.FindChild("LblName").GetComponent<UILabel>().text = info.firstName.Substring(0, 1) + ". " +info.lastName;
				if(info.teamName.Length < 1){
					tf.FindChild("LblTeam").GetComponent<UILabel>().text = UtilMgr.GetLocalText("StrInactive");
				} else
					tf.FindChild("LblTeam").GetComponent<UILabel>().text = info.city + " " + info.teamName;
			} else{
				tf.FindChild("LblName").GetComponent<UILabel>().text = info.korName;
				if(info.teamName.Length < 1){
					tf.FindChild("LblTeam").GetComponent<UILabel>().text = UtilMgr.GetLocalText("StrInactive");
				} else
					tf.FindChild("LblTeam").GetComponent<UILabel>().text = info.korTeamName;
			}




			
			tf.FindChild("LblSalary").GetComponent<UILabel>().text = "$"+info.salary;
			tf.FindChild("Star").FindChild("StarV").GetComponent<UILabel>().text = info.cardClass+"";
			tf.FindChild("Star").FindChild("StarV").localPosition = new Vector3(20f + (18f * (info.cardClass -1)), -4f);
			for(int i = 1; i <= 6; i++)
				tf.FindChild("Star").FindChild("SprStar"+i).gameObject.SetActive(false);
			for(int i = 1; i <= info.cardClass; i++){
				tf.FindChild("Star").FindChild("SprStar"+i).gameObject.SetActive(true);
				string starStr = "star_bronze";
				if(info.cardClass > 4){
					starStr = "star_gold";
				} else if(info.cardClass > 2){
					starStr = "star_silver";
				}
				tf.FindChild("Star").FindChild("SprStar"+i).GetComponent<UISprite>().spriteName = starStr;
			}

			tf.FindChild("Level").localPosition = new Vector3(-124f + (18f * (info.cardClass -1)), -40f);
			tf.FindChild("Level").FindChild("LblLevel").FindChild("LevelV").GetComponent<UILabel>().text = info.cardLevel+"";
			tf.FindChild("LblFPPG").FindChild("LblFPPGV").GetComponent<UILabel>().text = info.fppg;
			tf.FindChild("LblSkill").FindChild("LblSkillV").GetComponent<UILabel>().text = "1";
//			if(UtilMgr.IsMLB()){
//				tf.FindChild("MLB").gameObject.SetActive(true);
//				tf.FindChild("KBO").gameObject.SetActive(false);
//
//				tf = item.Target.transform.FindChild("ItemCard").FindChild("MLB");
//				
//				if((info.injuryYN != null) && (info.injuryYN.Equals("Y"))){
//					tf.FindChild("BtnPhoto").FindChild("SprInjury").gameObject.SetActive(true);
//				} else
//					tf.FindChild("BtnPhoto").FindChild("SprInjury").gameObject.SetActive(false);
//
//				if(info.useYn > 0){
//					tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Inuse").gameObject.SetActive(true);
//					tf.FindChild("BtnPhoto").FindChild("Panel").GetComponent<UIPanel>().baseClipRegion
//						= new Vector4(0, 0, 156f, 130f);
//				} else{
//					tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Inuse").gameObject.SetActive(false);
//					tf.FindChild("BtnPhoto").FindChild("Panel").GetComponent<UIPanel>().baseClipRegion
//						= new Vector4(0, 0, 152f, 108f);
//				}
//			} else{
				tf.FindChild("MLB").gameObject.SetActive(false);
				tf.FindChild("KBO").gameObject.SetActive(true);

				tf = item.Target.transform.FindChild("ItemCard").FindChild("KBO");

				if((info.injuryYN != null) && (info.injuryYN.Equals("Y"))){
					tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("SprInjury").gameObject.SetActive(true);
				} else
					tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("SprInjury").gameObject.SetActive(false);

				tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Inuse").gameObject.SetActive(false);
				tf.FindChild("BtnPhoto").FindChild("Panel").GetComponent<UIPanel>().baseClipRegion
					= new Vector4(0, 0, 152f, 186f);

				if(info.useYn > 0){
					tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Inuse").gameObject.SetActive(true);
					tf.FindChild("BtnPhoto").FindChild("Panel").GetComponent<UIPanel>().baseClipRegion
					= new Vector4(0, 0, 156f, 130f);
				} else{
					tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Inuse").gameObject.SetActive(false);
					tf.FindChild("BtnPhoto").FindChild("Panel").GetComponent<UIPanel>().baseClipRegion
					= new Vector4(0, 0, 152f, 108f);
				}
//			}



			tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().mainTexture
				= UtilMgr.GetTextureDefault();
			
			tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>().color
				= new Color(1f, 1f, 1f, 50f/255f);
			
			UtilMgr.LoadImage(info.playerFK,
			                  tf.FindChild("BtnPhoto").FindChild("Panel").FindChild("Texture").GetComponent<UITexture>());


		} else if(info.mType == CardInfo.INVEN_TYPE.PACK){
			item.Target.transform.FindChild("ItemCardPack").gameObject.SetActive(true);
			item.Target.transform.FindChild("ItemCard").gameObject.SetActive(false);
			item.Target.transform.FindChild("ItemExpand").gameObject.SetActive(false);

			item.Target.transform.FindChild("ItemCardPack").FindChild("LblName").GetComponent<UILabel>().text
				= item.Target.GetComponent<ItemInvenCard>().mCardInfo.mMailinfo.mail_title;
			item.Target.transform.FindChild("ItemCardPack").FindChild("LblDesc").GetComponent<UILabel>().text
				= item.Target.GetComponent<ItemInvenCard>().mCardInfo.mMailinfo.mail_desc;
		} else if(info.mType == CardInfo.INVEN_TYPE.EXPAND){
			item.Target.transform.FindChild("ItemCardPack").gameObject.SetActive(false);
			item.Target.transform.FindChild("ItemCard").gameObject.SetActive(false);
			item.Target.transform.FindChild("ItemExpand").gameObject.SetActive(true);
			item.Target.transform.FindChild("ItemExpand").FindChild("LblMoreCards1").GetComponent<UILabel>()
				.text = string.Format(UtilMgr.GetLocalText("LblMoreCards1"),
				                      mCardEvent.Response.data.Count - UserMgr.LobbyInfo.userInvenOfCard -1);
		}
	}

	public void ShowPlayerCard(){
		transform.root.FindChild("PlayerCard").gameObject.SetActive(true);
	}
}
