<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="webtrends.redirect.aspx.cs" Inherits="USDA_ARS.Umbraco.App_Plugins.UsdaWebtrendsRedirect.webtrends_redirect" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <script language="JavaScript">
        function submitForm() {
            document.wtForm.submit();
        }
    </script>
</head>
<body onload="submitForm()">
    <h2>Redirecting to WebTrends...</h2>

    <form name="wtForm" method="post"
        action="https://wt.arsnet.usda.gov/wrc/bin/OnDemandWRCReport/<%= PROFILE_ID %>">

        <input type="hidden" name="f1_username" value="SitePublisherUser">
        <input type="hidden" name="f1_password" value="VAC6?6hEgUga">
    </form>

</body>
