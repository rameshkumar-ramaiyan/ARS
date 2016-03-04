<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test-script.aspx.cs" Inherits="USDA_ARS.LocationsWebApp.test_script" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button id="btn_Test" runat="server" OnClick="btn_Test_Click" Text="Test" />
        <br />
        <br />
        <hr />
        <asp:Literal ID="output" runat="server" />
    </div>
    </form>
</body>
</html>
