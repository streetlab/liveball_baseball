﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSeasonInfo  {
	GraphInfo _graph;

	public GraphInfo graph {
		get {
			return _graph;
		}
		set {
			_graph = value;
		}
	}

	GraphInfo _statsAvg;

	public GraphInfo statsAvg {
		get {
			return _statsAvg;
		}
		set {
			_statsAvg = value;
		}
	}

	StatsInfo _stats;

	public StatsInfo stats {
		get {
			return _stats;
		}
		set {
			_stats = value;
		}
	}

	public class GraphInfo{
		string _positionNo;
		public string positionNo {
			get {
				return _positionNo;
			}
			set {
				_positionNo = value;
			}
		}

//"1",
		string _season;
		public string season {
			get {
				return _season;
			}
			set {
				_season = value;
			}
		}

//"2016",
		string _stamina;
		public string stamina {
			get {
				return _stamina;
			}
			set {
				_stamina = value;
			}
		}

//"0",
		string _riskMng;
		public string riskMng {
			get {
				return _riskMng;
			}
			set {
				_riskMng = value;
			}
		}

//"0",
		string _ballPower;
		public string ballPower {
			get {
				return _ballPower;
			}
			set {
				_ballPower = value;
			}
		}

//"0",
		string _control;
		public string control {
			get {
				return _control;
			}
			set {
				_control = value;
			}
		}

//"0",
		string _gameMng;
		public string gameMng {
			get {
				return _gameMng;
			}
			set {
				_gameMng = value;
			}
		}

//"0",
		string _contact;
		public string contact {
			get {
				return _contact;
			}
			set {
				_contact = value;
			}
		}

//"0",
		string _power;
		public string power {
			get {
				return _power;
			}
			set {
				_power = value;
			}
		}

//"0",
		string _battingEye;
		public string battingEye {
			get {
				return _battingEye;
			}
			set {
				_battingEye = value;
			}
		}

//"0",
		string _runSpeed;
		public string runSpeed {
			get {
				return _runSpeed;
			}
			set {
				_runSpeed = value;
			}
		}

//"0",
		string _concentration;//"0"

		public string concentration {
			get {
				return _concentration;
			}
			set {
				_concentration = value;
			}
		}
	}

	public class StatsInfo{		
		string _positionNo;
		public string positionNo {
			get {
				return _positionNo;
			}
			set {
				_positionNo = value;
			}
		}
		
		//"3",
		string _season;
		public string season {
			get {
				return _season;
			}
			set {
				_season = value;
			}
		}
		
		//"2015",
		string _BB;
		public string BB {
			get {
				return _BB;
			}
			set {
				_BB = value;
			}
		}
		
		//"10",
		string _AVG;
		public string AVG {
			get {
				return _AVG;
			}
			set {
				_AVG = value;
			}
		}
		
		//"0.2",
		string _HR;
		public string HR {
			get {
				return _HR;
			}
			set {
				_HR = value;
			}
		}
		
		//"5",
		string _RBI;
		public string RBI {
			get {
				return _RBI;
			}
			set {
				_RBI = value;
			}
		}
		
		//"24",
		string _R;
		public string R {
			get {
				return _R;
			}
			set {
				_R = value;
			}
		}
		
		//"14",
		string _H;
		public string H {
			get {
				return _H;
			}
			set {
				_H = value;
			}
		}
		
		//"42.1",
		string _OBP;
		public string OBP {
			get {
				return _OBP;
			}
			set {
				_OBP = value;
			}
		}
		
		//"0.3",
		string _SLG;
		public string SLG {
			get {
				return _SLG;
			}
			set {
				_SLG = value;
			}
		}
		
		//"0.4",
		string _OPS;
		public string OPS {
			get {
				return _OPS;
			}
			set {
				_OPS = value;
			}
		}
		
		//"0.7",
		string _SB;
		public string SB {
			get {
				return _SB;
			}
			set {
				_SB = value;
			}
		}
		
		//"1",
		string _CS;
		public string CS {
			get {
				return _CS;
			}
			set {
				_CS = value;
			}
		}
		
		//"0",
		string _HBP;
		public string HBP {
			get {
				return _HBP;
			}
			set {
				_HBP = value;
			}
		}
		
		//"0"
		string _W;
		
		public string W {
			get {
				return _W;
			}
			set {
				_W = value;
			}
		}
		
		string _L;
		
		public string L {
			get {
				return _L;
			}
			set {
				_L = value;
			}
		}
		
		string _SV;
		
		public string SV {
			get {
				return _SV;
			}
			set {
				_SV = value;
			}
		}
		
		string _IP;
		
		public string IP {
			get {
				return _IP;
			}
			set {
				_IP = value;
			}
		}
		
		string _ERA;
		
		public string ERA {
			get {
				return _ERA;
			}
			set {
				_ERA = value;
			}
		}
		
		string _PH;
		
		public string PH {
			get {
				return _PH;
			}
			set {
				_PH = value;
			}
		}
		
		string _ER;
		
		public string ER {
			get {
				return _ER;
			}
			set {
				_ER = value;
			}
		}
		
		string _SO;
		
		public string SO {
			get {
				return _SO;
			}
			set {
				_SO = value;
			}
		}
		
		string _PHR;
		
		public string PHR {
			get {
				return _PHR;
			}
			set {
				_PHR = value;
			}
		}
		
		string _WHIP;
		
		public string WHIP {
			get {
				return _WHIP;
			}
			set {
				_WHIP = value;
			}
		}
		
		string _PBAA;
		
		public string PBAA {
			get {
				return _PBAA;
			}
			set {
				_PBAA = value;
			}
		}
	}
	
}
