<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="news-import.aspx.cs" Inherits="USDA_ARS.LocationsWebApp.news_import" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button id="btn_Import" runat="server" OnClick="btn_Import_Click" Text="Import News" />
        <br />
        <br />
        <hr />
        <asp:Literal ID="output" runat="server" />
    </div>
    </form>
</body>
</html>
