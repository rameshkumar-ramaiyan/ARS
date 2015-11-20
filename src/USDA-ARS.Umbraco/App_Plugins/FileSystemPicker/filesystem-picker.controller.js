function fileSystemPickerController($scope, dialogService) {


    $scope.openPicker = function () {
        dialogService.open({
            template: "/App_Plugins/FileSystemPicker/filesystem-picker-dialog.html",
            callback: populate,
            dialogData: {
                filter: $scope.model.config.filter,
                folder: $scope.model.config.folder
            }
        });
    };

    $scope.remove = function () {
        $scope.model.value = "";
    };

    function populate(data) {
        $scope.model.value = $scope.model.config.folder + data;
        
    };
};
angular.module("umbraco").controller("Umbraco.FileSystemPickerController", fileSystemPickerController);

function folderSystemPickerController($scope, dialogService) {
    $scope.openPicker = function () {
        dialogService.open({
            template: "/App_Plugins/FileSystemPicker/foldersystem-picker-dialog.html",
            callback: populate
        });
    };
    function populate(data) {
        $scope.model.value = "/" + data;
    };

};
angular.module("umbraco").controller("Umbraco.FolderSystemPickerController", folderSystemPickerController);

function fileSystemPickerDialogController($scope, dialogService) {

    $scope.dialogEventHandler = $({});
    $scope.dialogEventHandler.bind("treeNodeSelect", nodeSelectHandler);

    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();

        if (args.node.icon !== "icon-folder")
            $scope.submit(args.node.id);
    };
};
angular.module("umbraco").controller("Umbraco.FileSystemPickerDialogController", fileSystemPickerDialogController);

function folderSystemPickerDialogController($scope, dialogService) {

    $scope.dialogEventHandler = $({});
    $scope.dialogEventHandler.bind("treeNodeSelect", nodeSelectHandler);

    function nodeSelectHandler(ev, args) {
        args.event.preventDefault();
        args.event.stopPropagation();
        $scope.submit(args.node.id);
        
    };
};
angular.module("umbraco").controller("Umbraco.FolderSystemPickerDialogController", folderSystemPickerDialogController);