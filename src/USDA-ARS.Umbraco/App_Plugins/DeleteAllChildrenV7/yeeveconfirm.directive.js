/**
 * @ngdoc directive
 * @name umbraco.directives.directive:umbConfirm
 * @function
 * @description
 * A confirmation dialog
 *
 * @restrict E
 */
function yeeveConfirmDirective() {
    return {
        restrict: "E",    // restrict to an element
        replace: true,   // replace the html element with the template
        templateUrl: '/App_Plugins/DeleteAllChildrenV7/yeeve-confirm.html',
        scope: {
            onConfirm: '=',
            onCancel: '=',
            caption: '@',
            disabled: '='
        },
        link: function (scope, element, attr, ctrl) {

        }
    };
}
angular.module('umbraco.directives').directive("yeeveConfirm", yeeveConfirmDirective);
