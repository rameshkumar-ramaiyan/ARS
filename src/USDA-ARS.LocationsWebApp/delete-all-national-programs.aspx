<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="delete-all-national-programs.aspx.cs" Inherits="USDA_ARS.LocationsWebApp.delete_all_national_programs" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Button id="btn_Delete" runat="server" OnClick="btn_Delete_Click" Text="Delete All National Program Items." />
        <br />
        <br />
        <hr />
        <asp:Literal ID="output" runat="server" />
    </div>
    </form>
</body>
</html>
