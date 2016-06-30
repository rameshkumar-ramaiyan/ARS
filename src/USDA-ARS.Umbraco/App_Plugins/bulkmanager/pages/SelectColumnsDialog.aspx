<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SelectColumnsDialog.aspx.cs" MasterPageFile="~/umbraco/masterpages/umbracoDialog.Master" Inherits="BulkManager.UI.Pages.SelectColumns" %>
<%@ Register TagPrefix="umbraco" Namespace="umbraco.uicontrols" Assembly="controls" %>
<%@ Register TagPrefix="BulkManager" Namespace="BulkManager.Core.Controls" Assembly="BulkManager.Core" %>
<asp:Content ID="header" ContentPlaceHolderID="head" runat="server">
   <BulkManager:CssRenderControl ID="CssRenderControl" runat="server"></BulkManager:CssRenderControl>
      <asp:PlaceHolder runat="server" ID="ProcessedPlaceHolder">
        <script type="text/javascript">
            var selectedColumns = '<%=GetSelectedColumns() %>';
            function columnsSelected() {

                UmbClientMgr.contentFrame().focus();
                UmbClientMgr.contentFrame().setSelectedColumnFields(selectedColumns);
                UmbClientMgr.contentFrame().clickSearchButton();
                UmbClientMgr.closeModalWindow();
                return false;
            }

            $(document).ready(function ($) {
                columnsSelected();
            });

        </script>
    </asp:PlaceHolder>

    </asp:Content>
<asp:Content ID="Content1" ContentPlaceHolderID="body" runat="server">
    <asp:ValidationSummary runat="server" ID="ValSum" CssClass="error"/>
    <BulkManager:HiddenTextBox runat="server" ID="HiddenTextBox"></BulkManager:HiddenTextBox>
    <asp:CustomValidator runat="server" ID="SelectedFieldsValidator" ControlToValidate="HiddenTextBox" ValidateEmptyText="True" CssClass="hidden"></asp:CustomValidator>
<umbraco:Pane ID="IntroPane" runat="server">
    <umbraco:PropertyPanel runat="server" ID="IntroPanel"><BulkManager:ResourceControl ID="IntroResource" ResourceKey="SelectColumnsDialog_Intro" runat="server"/></umbraco:PropertyPanel>
</umbraco:Pane>
    <umbraco:Pane ID="SelectColumnsPane" runat="server">
    <umbraco:PropertyPanel runat="server" ID="SelectColumnsPanel">
    <BulkManager:SelectColumnsControl runat="server" ID="SelectColumnsControl"></BulkManager:SelectColumnsControl>  
    </umbraco:PropertyPanel>
</umbraco:Pane>
    <umbraco:Pane runat="server" ID="ButtonPane">
        <umbraco:PropertyPanel runat="server" ID="ButtonsPanel">
            <asp:Button ID="SelectButton" runat="server"  CssClass="selectButton btn btn-success" OnClick="SelectButton_OnClick" ></asp:Button>
             &nbsp; <em><%= umbraco.ui.Text("or") %></em>&nbsp; <a href="#"  onclick="UmbClientMgr.closeModalWindow()" class="btn "><BulkManager:ResourceControl ID="CancelResource" ResourceKey="BulkManager_Buttons_Cancel" runat="server"/></a>
        </umbraco:PropertyPanel>
    </umbraco:Pane>
</asp:Content>