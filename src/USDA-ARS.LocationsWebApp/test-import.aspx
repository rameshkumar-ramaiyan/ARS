﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test-import.aspx.cs" Inherits="USDA_ARS.LocationsWebApp.test_import" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p><asp:Button ID="btnImport" runat="server" OnClick="btnImport_Click" Text="Test Import" /></p>
        <hr />
        <p>
            ID: <asp:TextBox ID="txtIdUpdate" runat="server" Text="8050" />
            Name: <asp:TextBox ID="txtName" runat="server" Text="Test Page Update" />
            <asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" Text="Update Content Page" />
        </p>
        <hr />
        <p>
            <asp:TextBox ID="txtId" runat="server" Text="8050" />
            <asp:Button ID="btnGet" runat="server" OnClick="btnGet_Click" Text="Get By Id" />
        </p>
        <hr />
        <p>
            <asp:TextBox ID="txtModeCode" runat="server" Text="20-00-00-00" />
            <asp:Button ID="btnGetByModeCode" runat="server" OnClick="btnGetByModeCode_Click" Text="Get By ModeCode" />
        </p>
        <hr />
        <p>
            <asp:TextBox ID="txtParentId" runat="server" Text="8050" />
            <asp:Button ID="btnGetChild" runat="server" OnClick="btnGetChild_Click" Text="Get Child Content List" />
        </p>
        <hr />
        <h2>Response</h2>
        <asp:Literal ID="output" runat="server" />
        <hr />
        <h2>Child Content</h2>
        <asp:Literal ID="outputChild" runat="server" />
    </div>
    </form>
</body>
</html>
