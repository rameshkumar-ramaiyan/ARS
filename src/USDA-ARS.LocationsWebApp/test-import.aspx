<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test-import.aspx.cs" Inherits="USDA_ARS.LocationsWebApp.test_import" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button ID="btnImport" runat="server" OnClick="btnImport_Click" Text="Test Import" />
        <hr />
        <asp:Literal ID="output" runat="server" />
    </div>
    </form>
</body>
</html>
