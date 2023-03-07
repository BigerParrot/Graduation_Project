using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

using SEComEnabler.SEComStructure;

namespace VirtualMES
{
    public class BaseBiz
    {
        public static SXTransaction SendS6F12(int ceid, SEComData sd)
        {
            try
            {
                SXTransaction sxTrx = new SXTransaction();
                sxTrx.Stream = 6;
                sxTrx.Function = 12;
                sxTrx.Direction = SX.SECSDirection.FromHost;

                if (ceid == 101)
                {
                    int work_type = sd.DataItems[0][1][2].Value;
                    string response = string.Empty;
                    string fromTrack = string.Empty;
                    string toTrack = string.Empty;
                    string bottomTrayId = string.Empty;
                    string topTrayId = string.Empty;

                    if (work_type == 1)
                        response = "4";
                    else if (work_type == 3) // 이동
                    {
                        response = "1";
                        fromTrack = sd.DataItems[0][1][1].Value;
                        bottomTrayId = sd.DataItems[0][1][3].Value;
                        topTrayId = sd.DataItems[0][1][4].Value;
                    }

                    sxTrx.WriteNode(SX.SECSFormat.L, 6, "", "");
                    sxTrx.WriteNode(SX.SECSFormat.U2, 1, "0", "ERACK6");
                    sxTrx.WriteNode(SX.SECSFormat.U2, 1, ceid.ToString(), "CEID");
                    sxTrx.WriteNode(SX.SECSFormat.A, 5, fromTrack, "FROM_TRACK");
                    sxTrx.WriteNode(SX.SECSFormat.A, 5, "", "TRACK_NO");    // 이동할 곳을 찾아와야 함..
                    sxTrx.WriteNode(SX.SECSFormat.U2, 1, response, "RESPONSE");
                    sxTrx.WriteNode(SX.SECSFormat.L, 1, "", "");
                    sxTrx.WriteNode(SX.SECSFormat.L, 5, "", "");
                    sxTrx.WriteNode(SX.SECSFormat.A, 6, bottomTrayId, "BOTTOM_TRAYID");
                    sxTrx.WriteNode(SX.SECSFormat.A, 6, topTrayId, "TOP_TRAYID");
                    sxTrx.WriteNode(SX.SECSFormat.A, 11, "", "LOCATION");
                    sxTrx.WriteNode(SX.SECSFormat.A, 14, "", "TIME");
                    sxTrx.WriteNode(SX.SECSFormat.A, 9, "", "DURATION");
                }

                return sxTrx;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.GetBaseException().ToString());
                return null;
            }
        }

        public static SXTransaction MakeReplyS1F14(SEComData sd)
        {
            try
            {
                SXTransaction sxTrx = new SXTransaction();
                sxTrx.Stream = 1;
                sxTrx.Function = 14;
                sxTrx.Direction = SX.SECSDirection.FromHost;
                
                string mdln = sd.DataItems[0][1][0].Value;
                string softRev = sd.DataItems[0][1][1].Value;

                sxTrx.WriteNode(SX.SECSFormat.L, 2, "", "");
                sxTrx.WriteNode(SX.SECSFormat.B, 1, "0", "COMMACK");
                sxTrx.WriteNode(SX.SECSFormat.L, 2, "", "");
                sxTrx.WriteNode(SX.SECSFormat.A, 4, mdln.PadRight(4), "MDLN");
                sxTrx.WriteNode(SX.SECSFormat.A, 6, softRev.PadRight(6), "SOFTREV");

                return sxTrx;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.GetBaseException().ToString());
                return null;
            }
        }

        public static SXTransaction MakeS1F13()
        {
            try
            {
                SXTransaction sxTrx = new SXTransaction();
                sxTrx.Stream = 1;
                sxTrx.Function = 13;
                sxTrx.Direction = SX.SECSDirection.FromHost;
                sxTrx.Wait = true;
                
                sxTrx.WriteNode(SX.SECSFormat.L, 2, "", "");
                sxTrx.WriteNode(SX.SECSFormat.A, 4, "    ", "MDLN");
                sxTrx.WriteNode(SX.SECSFormat.A, 6, "      ", "SOFTREV");    // 이동할 곳을 찾아와야 함..
                
                return sxTrx;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.GetBaseException().ToString());
                return null;
            }
        }

        public static string FindDestTrackNo(string sourceTrackNo)
        {
            string retValue = string.Empty;

            IEnumerable<DataRow>  drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                  where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                  select equipRoute;

            foreach (DataRow dr in drs)
            {
                retValue = dr["DEST_TR_NO"].ToString();
                return retValue;
            }

            return retValue;
        }

        public static string FindDestTrackNo(string sourceTrackNo, string trayId)
        {
            string retValue = string.Empty;
            string fillterExpression = String.Empty;
            string sortOrder = String.Empty;
            IEnumerable<DataRow> drs = null;

            if (String.IsNullOrEmpty(trayId))
            {
                // 기본 목적지
                //fillterExpression = String.Format("T0_NUMBER(SOUR_TR_NO) = T0_NUMBER('{0}')", sourceTrackNo);
                //drs = frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.Select(fillterExpression);
                drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                      where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                      select equipRoute;
            }
            else
            {
                // 트레이 목적지
                // 트레이 ID 범위 값으로 지정
                //fillterExpression = String.Format("T0_NUMBER(SOUR_TR_NO) = T0_NUMBER('{0}') AND TO_TRAY_ID >= '{1}' AND FROM_TRAY_ID <= '{2}'", sourceTrackNo, trayId.Substring(1), trayId.Substring(1));
                //sortOrder = "SEQ ASC";
                //drs = frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.Select(fillterExpression, sortOrder);
                drs = from trayRoute in frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.AsEnumerable()
                      where Convert.ToInt32(trayRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim()) &&
                            Convert.ToInt32(trayRoute.Field<string>("TO_TRAY_ID")) >= Convert.ToInt32(trayId.Substring(1).Trim()) &&
                            Convert.ToInt32(trayRoute.Field<string>("FROM_TRAY_ID")) <= Convert.ToInt32(trayId.Substring(1).Trim())
                      select trayRoute;

                if (drs == null || drs.Count() == 0)
                {
                    // 기본 목적지
                    drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                          where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                          select equipRoute;
                }
            }

            foreach (DataRow dr in drs)
            {
                retValue = dr["DEST_TR_NO"].ToString();
                return retValue;
            }

            return retValue;
        }

        public static string FindDestTrackNo(string sourceTrackNo, string trayId, string type)
        {
            string retValue = string.Empty;
            string fillterExpression = String.Empty;
            string sortOrder = String.Empty;
            IEnumerable<DataRow> drs = null;

            if (String.IsNullOrEmpty(trayId))
            {
                // 기본 목적지
                drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                      where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                      select equipRoute;
            }
            else
            {
                // 트레이 목적지
                // 트레이 ID 범위 값으로 지정
                if (type.Equals("OUT"))
                {
                    //fillterExpression = String.Format("SOUR_TR_NO LIKE '{0}%' AND TO_TRAY_ID >= '{1}' AND FROM_TRAY_ID <= '{2}' AND TYPE = 'OUT'", sourceTrackNo.Substring(0, 3), trayId.Substring(1), trayId.Substring(1));
                    drs = from trayRoute in frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.AsEnumerable()
                          where trayRoute.Field<string>("SOUR_TR_NO").Substring(0, 3) == sourceTrackNo.Trim().Substring(0, 3) &&
                                Convert.ToInt32(trayRoute.Field<string>("TO_TRAY_ID")) >= Convert.ToInt32(trayId.Substring(1).Trim()) &&
                                Convert.ToInt32(trayRoute.Field<string>("FROM_TRAY_ID")) <= Convert.ToInt32(trayId.Substring(1).Trim())
                          select trayRoute;
                }
                else
                {
                    //fillterExpression = String.Format("T0_NUMBER(SOUR_TR_NO) = T0_NUMBER('{0}') AND TO_TRAY_ID >= '{1}' AND FROM_TRAY_ID <= '{2}'", sourceTrackNo, trayId.Substring(1), trayId.Substring(1));
                    drs = from trayRoute in frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.AsEnumerable()
                          where Convert.ToInt32(trayRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim()) &&
                                Convert.ToInt32(trayRoute.Field<string>("TO_TRAY_ID")) >= Convert.ToInt32(trayId.Substring(1).Trim()) &&
                                Convert.ToInt32(trayRoute.Field<string>("FROM_TRAY_ID")) <= Convert.ToInt32(trayId.Substring(1).Trim())
                          select trayRoute;
                }

                //sortOrder = "SEQ ASC";
                //drs = frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.Select(fillterExpression, sortOrder);

                if (drs == null || drs.Count() == 0)
                {
                    // 기본 목적지
                    drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                          where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                          select equipRoute;
                }
            }

            foreach (DataRow dr in drs)
            {
                retValue = dr["DEST_TR_NO"].ToString();
                return retValue;
            }

            return retValue;
        }

        public static string FindDestMCType(string sourceTrackNo)
        {
            string retValue = string.Empty;

            IEnumerable<DataRow>  drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                  where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                  select equipRoute;

            foreach (DataRow dr in drs)
            {
                retValue = dr["MC_TYPE"].ToString();
                return retValue;
            }

            return retValue;
        }

        public static string FindDestMCType(string sourceTrackNo, string trayId)
        {
            string retValue = string.Empty;
            string fillterExpression = String.Empty;
            string sortOrder = String.Empty;
            IEnumerable<DataRow> drs = null;

            if (String.IsNullOrEmpty(trayId))
            {
                //// 기본 목적지
                //fillterExpression = String.Format("T0_NUMBER(SOUR_TR_NO) = T0_NUMBER('{0}')", sourceTrackNo);
                //drs = frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.Select(fillterExpression);
                drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                      where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                      select equipRoute;

                foreach (DataRow dr in drs)
                {
                    retValue = dr["MC_TYPE"].ToString();
                    return retValue;
                }
            }
            else
            {
                //// 트레이 목적지
                //// 트레이 ID 범위 값으로 지정
                drs = from trayRoute in frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.AsEnumerable()
                    where Convert.ToInt32(trayRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                    select trayRoute;

                foreach (DataRow dr in drs)
                {
                    retValue = dr["TYPE"].ToString();

                    if (retValue.Equals("IN"))
                        retValue = "MC";

                    return retValue;
                }
            }

            return retValue;
        }

        public static DataRow FindDestRowData(string sourceTrackNo, string trayId)
        {
            string retValue = string.Empty;
            string fillterExpression = String.Empty;
            string sortOrder = String.Empty;
            IEnumerable<DataRow> drs = null;

            if (String.IsNullOrEmpty(trayId))
            {
                //// 기본 목적지
                //fillterExpression = String.Format("T0_NUMBER(SOUR_TR_NO) = T0_NUMBER('{0}')", sourceTrackNo);
                //drs = frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.Select(fillterExpression);
                drs = from equipRoute in frmMain.TB_EQUIP_INF_ROUTE.dtEquipInfRoute.AsEnumerable()
                      where Convert.ToInt32(equipRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                      select equipRoute;

                foreach (DataRow dr in drs)
                {
                    return dr;
                }
            }
            else
            {
                //// 트레이 목적지
                //// 트레이 ID 범위 값으로 지정
                drs = from trayRoute in frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.AsEnumerable()
                      where Convert.ToInt32(trayRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim())
                      select trayRoute;

                foreach (DataRow dr in drs)
                {
                    return dr;
                }
            }

            return null;
        }

        public static string CheckMGType(string sourceTrackNo, string trayId)
        {
            string retValue = string.Empty;
            string fillterExpression = String.Empty;
            string sortOrder = String.Empty;
            IEnumerable<DataRow> drs = null;
            
            //// 트레이 목적지
            //// 트레이 ID 범위 값으로 지정
            drs = from trayRoute in frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.AsEnumerable()
                    where Convert.ToInt32(trayRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim()) &&
                        Convert.ToInt32(trayRoute.Field<string>("TO_TRAY_ID")) >= Convert.ToInt32(trayId.Substring(1).Trim()) &&
                        Convert.ToInt32(trayRoute.Field<string>("FROM_TRAY_ID")) <= Convert.ToInt32(trayId.Substring(1).Trim())
                  select trayRoute;

            foreach (DataRow dr in drs)
            {
                retValue = dr["MG"].ToString();
                return retValue;
            }

            return retValue;
        }

        public static string CheckDPType(string sourceTrackNo, string trayId)
        {
            string retValue = string.Empty;
            string fillterExpression = String.Empty;
            string sortOrder = String.Empty;
            IEnumerable<DataRow> drs = null;

            //// 트레이 목적지
            //// 트레이 ID 범위 값으로 지정
            drs = from trayRoute in frmMain.TB_TRAY_INF_ROUTE.dtTrayInfRoute.AsEnumerable()
                  where Convert.ToInt32(trayRoute.Field<string>("SOUR_TR_NO")) == Convert.ToInt32(sourceTrackNo.Trim()) &&
                        Convert.ToInt32(trayRoute.Field<string>("TO_TRAY_ID")) >= Convert.ToInt32(trayId.Substring(1).Trim()) &&
                        Convert.ToInt32(trayRoute.Field<string>("FROM_TRAY_ID")) <= Convert.ToInt32(trayId.Substring(1).Trim())
                  select trayRoute;

            foreach (DataRow dr in drs)
            {
                retValue = dr["DP"].ToString();
                return retValue;
            }

            return retValue;
        }
    }
}
