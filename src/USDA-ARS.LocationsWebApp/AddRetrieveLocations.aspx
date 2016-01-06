<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddRetrieveLocations.aspx.cs" Inherits="USDA_ARS.LocationsWebApp.AddRetrieveLocations" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        .auto-style1 {
            width: 100%;
        }
        .auto-style2 {
            height: 64px;
        }
        .auto-style3 {
            height: 31px;
        }
        .auto-style4 {
            height: 40px;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <table class="auto-style1">
            <tr>
                <td>
                    <table class="auto-style1">
                        <tr>
                            <td colspan="2">
                                <h1>Area</h1>
                            </td>
                        </tr>
                        <tr>
                            <td rowspan="2">
                                <asp:Button ID="btnAreaRetrieveAll" runat="server" OnClick="btnAreaRetrieveAll_Click" Text="RetrieveAll" />
                            </td>
                            <td>
                                &nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>
                                &nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </td>
                <td>
                    <table class="auto-style1">
                        <tr>
                            <td colspan="2">
                                <h1>City</h1>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <asp:Button ID="btnCityRetrieveAll" runat="server" Text="RetrieveAll" OnClick="btnCityRetrieveAll_Click" />
                            </td>
                            <td>
                                <table class="auto-style1">
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblParentAreaModeCode" runat="server" Text="Area Mode Code"></asp:Label>
                                        </td>
                                        <td>&nbsp;</td>
                                        <td>&nbsp;</td>
                                    </tr>
                                    <tr>
                                        <td>
                                <asp:TextBox ID="txtParentAreaModeCode" runat="server"></asp:TextBox>
                                        </td>
                                        <td>&nbsp;</td>
                                        <td>&nbsp;</td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                &nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </td>
                <td>
                    <table class="auto-style1">
                        <tr>
                            <td colspan="2">
                                <h1>Research Center</h1>
                            </td>
                        </tr>
                        <tr>
                            <td class="auto-style2">
                                <asp:Button ID="btnResearchCenterRetrieveAll" runat="server" Text="RetrieveAll" OnClick="btnResearchCenterRetrieveAll_Click" />
                            </td>
                            <td class="auto-style2">
                                <table class="auto-style1">
                                    <tr>
                                        <td>
                                            <asp:Label ID="lblParentAreaModeCode0" runat="server" Text="Area Mode Code"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblParentAreaModeCode1" runat="server" Text="City Mode Code"></asp:Label>
                                        </td>
                                        <td>
                                            <asp:Label ID="lblParentAreaModeCode2" runat="server" Text="Facility Mode Code (optional)"></asp:Label>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td>
                                <asp:TextBox ID="txtRCParentAreaModeCode" runat="server"></asp:TextBox>
                                        </td>
                                        <td>
                                <asp:TextBox ID="txtRCParentCityModeCode" runat="server"></asp:TextBox>
                                        </td>
                                        <td>
                                <asp:TextBox ID="txtParenRCModeCode" runat="server"></asp:TextBox>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>
                                &nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                        <tr>
                            <td>&nbsp;</td>
                            <td>&nbsp;</td>
                        </tr>
                    </table>
                </td>
            </tr>
            <tr>
                <td class="auto-style4">
                    <asp:Label ID="lblMessage" runat="server" Text="Click RetrieveAll  to see the number of locations retrieved."></asp:Label>
                </td>
                <td class="auto-style4">
                    <asp:GridView ID="gvAreas" runat="server">
                    </asp:GridView>
                </td>
                <td class="auto-style4">
                                            &nbsp;</td>
            </tr>
            <tr>
                <td class="auto-style4">
                    &nbsp;</td>
                <td class="auto-style4">
                                            &nbsp;</td>
                <td class="auto-style4">
                                            &nbsp;</td>
            </tr>
            <tr>
                <td class="auto-style4">
                    &nbsp;</td>
                <td class="auto-style4">
                                            &nbsp;</td>
                <td class="auto-style4">
                                            &nbsp;</td>
            </tr>
            <tr>
                <td class="auto-style4">
                                            <asp:Label ID="lblParentAreaModeCode3" runat="server" Text="New Mode Code"></asp:Label>
                                <asp:TextBox ID="txtNewModeCode" runat="server"></asp:TextBox>
                </td>
                <td class="auto-style4">
                                            &nbsp;</td>
                <td class="auto-style4">
                                <asp:Button ID="btnAddNewArea" runat="server" OnClick="btnAddNewArea_Click" Text="Add/Update Area" />
                </td>
            </tr>
            <tr>
                <td>
                                            <asp:Label ID="lblParentAreaModeCode4" runat="server" Text="Old Mode Code"></asp:Label>
                                <asp:TextBox ID="txtOldModeCode" runat="server"></asp:TextBox>
                </td>
                <td>
                                            &nbsp;</td>
                <td>
                                <asp:Button ID="btnAddNewCity" runat="server" OnClick="btnAreaRetrieveAll_Click" Text="Add New City" />
                </td>
            </tr>
            <tr>
                <td>
                                            <asp:Label ID="lblParentAreaModeCode5" runat="server" Text="Old Id"></asp:Label>
                                <asp:TextBox ID="txtOldId" runat="server"></asp:TextBox>
                </td>
                <td>
                                            &nbsp;</td>
                <td>
                                &nbsp;</td>
            </tr>
            <tr>
                <td class="auto-style3">
                                            <asp:Label ID="lblParentAreaName" runat="server" Text="Area Name"></asp:Label>
                                <asp:TextBox ID="txtParentAreaName" runat="server" ></asp:TextBox>
                </td>
                <td class="auto-style3">
                                            &nbsp;</td>
                <td class="auto-style3">
                                <asp:Button ID="btnAddNewRC" runat="server" OnClick="btnAreaRetrieveAll_Click" Text="Add New Research Center" />
                                        </td>
            </tr>
            <tr>
                <td class="auto-style3">
                                            <asp:Label ID="lblParentCityName0" runat="server" Text="City Name"></asp:Label>
                                <asp:TextBox ID="txtParentCityName" runat="server"></asp:TextBox>
                </td>
                <td class="auto-style3">&nbsp;</td>
                <td class="auto-style3">&nbsp;</td>
            </tr>
            <tr>
                <td class="auto-style3">
                                            <asp:Label ID="lblParentCityName1" runat="server" Text="Facility Name"></asp:Label>
                                <asp:TextBox ID="txtRCParentCityModeCode2" runat="server"></asp:TextBox>
                </td>
                <td class="auto-style3"></td>
                <td class="auto-style3"></td>
            </tr>
            <tr>
                <td>
        <asp:Literal ID="output1" runat="server" />
                </td>
                <td>&nbsp;</td>
                <td>
                    &nbsp;</td>
            </tr>
        </table>
    </form>
</body>
</html>

