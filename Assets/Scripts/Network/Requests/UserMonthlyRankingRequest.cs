﻿using UnityEngine;
using System.Collections;
using System.Text;

public class UserMonthlyRankingRequest : BaseRequest {

	public UserMonthlyRankingRequest()
	{
		Add ("lastId", 0);

		mDic = this;
	}

	public override string GetType ()
	{
		return "apps.contest";
	}

	public override string GetQueryId()
	{
		return "getMonthlyRank";
	}

}
