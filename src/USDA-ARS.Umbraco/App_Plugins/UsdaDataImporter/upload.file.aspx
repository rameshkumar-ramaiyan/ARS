<%@ Import namespace="Umbraco.Core.Models" %>
<%@ Import namespace="Umbraco.Web" %>

<script language="c#" runat="server">
    public string Id = "";
    public string defaultTableName = "Schedule Tables";

    public void Page_Load(object sender, EventArgs e)
    {
        Id = Request.QueryString.Get("id");
        IPublishedContent siteSettings = USDA_ARS.Umbraco.Extensions.Helpers.Nodes.SiteSettings();

        if (siteSettings != null)
        {
            defaultTableName = siteSettings.GetPropertyValue<string>("contentTable");
        }
        
    }
</script>

<!DOCTYPE html>
<html>
<head>
    <title></title>
    <meta charset="utf-8" />
    <link href="/umbraco/assets/css/umbraco.css?cdv=509615776" type="text/css" rel="stylesheet" />
    <link href="/umbraco_client/tree/treeicons.css?cdv=509615776" type="text/css" rel="stylesheet" />
    <link href="/umbraco/lib/bootstrap-social/bootstrap-social.css?cdv=509615776" type="text/css" rel="stylesheet" />
    <link href="/umbraco/lib/font-awesome/css/font-awesome.min.css?cdv=509615776" type="text/css" rel="stylesheet" />
    <script></script>
</head>
<body>
    <form method="post" enctype="multipart/form-data" action="/umbraco/usda/DataImporter/Post">
        <div class="row control-group">
            <p>Please upload the OSQR Access database.</p>
            <p><em>Please save your changes to the page before uploading the file.</em></p>
        </div>
        <div class="row control-group">
            <input name="dataFile" id="dataFile" type="file" />
        </div>
        <div class="row control-group">
            <strong>Access Table Name</strong><br />
            <input name="tableName" id="tableName" type="text" value="<%= defaultTableName %>" />
            <input name="nodeId" id="nodeId" type="hidden" value="<%= Id %>" />
        </div>
        <div class="row control-group">
            <input name="uploadFile" id="uploadFile" type="submit" value="Process File" />
        </div>
    </form>
</body>
</html>
