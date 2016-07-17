using ClientDependency.Core;
using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.PropertyEditors;

namespace Axial.Umbraco.SiteDescendantsAudit
{
    [PropertyEditor("SiteDescendantsAudit", "Site Descendants Audit", "/App_Plugins/SiteDescendantsAudit/views/sitedescendantsaudit-property.html", ValueType = "TEXT")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/SiteDescendantsAudit/sitedescendantsaudit-resource.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/SiteDescendantsAudit/controllers/sitedescendantsaudit-config-controller.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/SiteDescendantsAudit/controllers/sitedescendantsaudit-property-controller.js")]
    [PropertyEditorAsset(ClientDependencyType.Javascript, "/App_Plugins/SiteDescendantsAudit/vendors/jquery-treetable.js")]

    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/SiteDescendantsAudit/sitedescendantsaudit.css")]
    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/SiteDescendantsAudit/vendors/jquery-treetable.css")]
    [PropertyEditorAsset(ClientDependencyType.Css, "/App_Plugins/SiteDescendantsAudit/vendors/jquery-treetable-theme-default.css")]
    public class DescendantsAuditPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new SiteDescendantsAuditPreValueEditor();
        }
                
        internal class SiteDescendantsAuditPreValueEditor : PreValueEditor
        {
            public SiteDescendantsAuditPreValueEditor()
            {                
                Fields.Add(new PreValueField()
                {
                    Description = "Comma seperated list of content type aliases.",
                    Key = "siteDescendantContentTypes",
                    Name = "Site Descendant Content Types",
                    View = "textstring"
                });
            }
        }
    }
}
