<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AddUpdateLocations.aspx.cs" Inherits="USDA_ARS.LocationsWebApp.AddUpdateLocations" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <style>
.table - main { width: 100%; } 
.table-main-row { width: 100%; clear: both; } 
.table-main-header { background-color: #ddd; padding: 3px;  }
.table-main-cell-left { width: 55%; float: left; padding-top: 3px;padding-bottom: 3px; }
.table-main-cell-right { width: 40%; float: left;  padding-top: 3px;padding-bottom: 3px; }
.table-main-cell-full { width: 100%; float: left;  padding-bottom: 3px; }
.table-main-cell-center { width: 100%; float: center;  padding-top: 3px;padding-bottom: 3px; }
.table
{
 display: table;
}
.tablerow
{
display: table-row; 
}
.tablecell
{
display: table-cell;
}
</style>
    <div >
       <div class="table-main-cell-center" align="CENTER" >
         <hr />
        <h1>Add-- Update --Retrieve Locations</h1>
        <hr />
        </div>
         <div class="table-main-row" > 
         <div class="table-main-header" align="CENTER" >
        <h1>1.RETRIEVALS</h1>
        
             </div>
         <div>
             <hr />
            <asp:TextBox ID="txtId" runat="server" Text="8050" />
            <asp:Button ID="btnGet" runat="server" OnClick="btnGet_Click" Text="Get By Id" />
        </div>
        <hr />
        <div>
            <asp:TextBox ID="txtModeCode" runat="server" Text="20-00-00-00" />
            <asp:Button ID="btnGetByModeCode" runat="server" OnClick="btnGetByModeCode_Click" Text="Get By ModeCode" />
        </div>
        <hr />
        <div>
            <asp:TextBox ID="txtParentId" runat="server" Text="8050" />
            <asp:Button ID="btnGetChild" runat="server" OnClick="btnGetChild_Click" Text="Get Child Content List" />
        &nbsp;</div>
         <hr />
       </div> 
         <div class="table-main-header" align="CENTER" >
        <h1>2.ADD LOCATIONS</h1>      
            </div>    
        

         <div class="table-main-row"  bordercolor="#FFFFFF">
        <div class="tablecell"  width = "25%" >
            <div class="table">
                <div class="table-main-row">
                    <div class="tablecell">       
                        <p><b><font face="Arial, Helvetica, sans-serif" size="2">Area</font>
                <asp:Button ID="btnAddMultipleAreas" runat="server" Text="Add Multiple Areas" OnClick="btnAddMultipleAreas_Click" />  
                            <asp:Button ID="getSoftwareFoldersFiles" runat="server" OnClick="getSoftwareFoldersFiles_Click" Text="getSoftwareFoldersFiles" />
                            </b>  
                            <asp:TextBox ID="txtgetSoftwareFoldersFiles" runat="server"></asp:TextBox>
                        </p>
                        </div>
                          </div>
                <br />
                 <div class="table-main-row"> 
                    <div class=" table-main-cell-left">New Mode Code</div><div class="tablecell">  <asp:TextBox ID="txtNewModeCode" runat="server"></asp:TextBox></div>                    
                         </div> 
                <div class="table-main-row"> 
				
                    <div class=" table-main-cell-left">Area Name </div><div class="tablecell"> <asp:TextBox ID="txtParentAreaName" runat="server"></asp:TextBox></div></div>
                    
                <br />
                     <div class="table-main-row">
                    <div class="tablecell">       
                        <p><b><font face="Arial, Helvetica, sans-serif" size="1">Optional Parameters</font> </b>  </p>
                        </div>
                          </div>
                  <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Old Mode Code(Site Publisher)</div><div class="tablecell">  <asp:TextBox ID="txtOldModeCode" runat="server"></asp:TextBox></div>
                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Old Id(Site Publisher)</div><div class="tablecell">  <asp:TextBox ID="txtOldId" runat="server"></asp:TextBox></div>
                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class="tablecell"><asp:Button ID="btnAddNewArea" runat="server" Text="Add New Area" OnClick="btnAddNewArea_Click" />  </div>
                     
                         </div> 

                             </div>  

                                </div>
              
       <div class="tablecell"  width = "25%" >
            <div class="table">
                <div class="table-main-row">
                    <div class="tablecell">       
                        <p><b><font face="Arial, Helvetica, sans-serif" size="2">City</font> </b>  </p>
                        </div>
                          </div>
                <br />
                <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Parent Id</div><div class="tablecell">  <asp:TextBox ID="txtParentAreaId" runat="server"></asp:TextBox></div>                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Parent Mode Code</div><div class="tablecell">  <asp:TextBox ID="txtParentAreaModeCode" runat="server"></asp:TextBox></div>                    
                         </div> 
                
                    
                <div class="table-main-row"> 
				
                    <div class=" table-main-cell-left">City Name </div><div class="tablecell"> <asp:TextBox ID="txtCityName" runat="server"></asp:TextBox></div></div>
                    
                 <div class="table-main-row"> 
				
                    <div class=" table-main-cell-left">State Code </div><div class="tablecell"> <asp:TextBox ID="txtStateCode" runat="server"></asp:TextBox></div></div>
                    <br />
               <div class="table-main-row">
                    <div class="tablecell">       
                        <p><b><font face="Arial, Helvetica, sans-serif" size="1">Optional Parameters</font> </b>  </p>
                        </div>
                          </div>
                  <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Old Mode Code(Site Publisher)</div><div class="tablecell">  <asp:TextBox ID="txtCityOldModeCodeSP" runat="server"></asp:TextBox></div>
                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Old Id(Site Publisher)</div><div class="tablecell">  <asp:TextBox ID="txtCityOldIdSP" runat="server"></asp:TextBox></div>
                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class="tablecell"><asp:Button ID="btnNewCity" runat="server" Text="Add New City" OnClick="btnAddNewCity_Click" />  </div>
                     
                         </div> 

                             </div>  

                                </div>
              
            <div class="tablecell"  width = "25%" >
            <div class="table">
                <div class="table-main-row">
                    <div class="tablecell">       
                        <p><b><font face="Arial, Helvetica, sans-serif" size="2">Research Center</font> </b>  </p>
                        </div>
                          </div>
                <br />
                 <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Parent Id</div><div class="tablecell">  <asp:TextBox ID="TextBox3" runat="server"></asp:TextBox></div>                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Parent Mode Code</div><div class="tablecell">  <asp:TextBox ID="TextBox4" runat="server"></asp:TextBox></div>                    
                         </div> 
                <div class="table-main-row"> 
				
                    <div class=" table-main-cell-left">RC Name </div><div class="tablecell"> <asp:TextBox ID="TextBox1" runat="server"></asp:TextBox></div></div>
                    
                    
                <div class="table-main-row"> 
				
                    <div class=" table-main-cell-left">City Name </div><div class="tablecell"> <asp:TextBox ID="TextBox11" runat="server"></asp:TextBox></div></div>
                    
                 <div class="table-main-row"> 
				
                    <div class=" table-main-cell-left">State Code </div><div class="tablecell"> <asp:TextBox ID="TextBox12" runat="server"></asp:TextBox></div></div>
                    <br />
                
                     <div class="table-main-row">
                    <div class="tablecell">       
                        <p><b><font face="Arial, Helvetica, sans-serif" size="1">Optional Parameters</font> </b>  </p>
                        </div>
                          </div>
                  <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Old Mode Code(Site Publisher)</div><div class="tablecell">  <asp:TextBox ID="TextBox7" runat="server"></asp:TextBox></div>
                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class=" table-main-cell-left">Old Id(Site Publisher)</div><div class="tablecell">  <asp:TextBox ID="TextBox8" runat="server"></asp:TextBox></div>
                    
                         </div> 
                 <div class="table-main-row"> 
                    <div class="tablecell"><asp:Button ID="btnAddNewRC" runat="server" Text="Add New Research Center" OnClick="btnAddNewRC_Click" />  </div>
                     
                         </div> 

                             </div>  

                                </div>
              
       <div class="tablecell"  width = "25%" >
            
                                    </div>
             </div>
					 			
									
									
       
        <hr />
          <div class="table-main-header" align="CENTER" >
         <h1>3.UPDATE LOCATIONS</h1>         
            </div>
        <hr />
         <div>
            ID: <asp:TextBox ID="txtIdUpdate" runat="server" Text="8050" />
            Name: <asp:TextBox ID="txtName" runat="server" Text="Test Page Update" />
            <asp:Button ID="btnUpdate" runat="server" OnClick="btnUpdate_Click" Text="Update Content Page" />
            
             <asp:HyperLink ID="lnkDeleteLocations" runat="server" NavigateUrl="~/delete-regions.aspx">Delete All Locations</asp:HyperLink>
            
        </div>
        <hr />
        <div class="table-main-header" align="CENTER" >
       <h1>4.RESULTS</h1>
       
       
           </div>
        <hr />
        <h2>Response</h2>
        <asp:Literal ID="output" runat="server" />
        <hr />
        <h2>Child Content</h2>
        <asp:Literal ID="outputChild" runat="server" />
        <hr />
    </div>
    </form>
</body>
</html>
