var App = angular
.module('MyApp', '[datatables]');
App.controller('UserCtr', ['$sope', '$http', 'DTOptionsBuilder', 'DTOColumnBuilder', function ($scope, $http, DTOptionsBuilder, DTOColumnBuilder) {
    $scope.dtColumns = [
        DTOColumnBuilder.newColumn("FirstName", "FirstName"),
        DTOColumnBuilder.newColumn("LastName", "LastName")

    ]
    $scope.dtOption = DTOptionsBuilder.newOptions().withOption('ajax', {
        url: "/User/DataTable",
        type: "POST"

    })
    .with.paginationType('full numbers')
    .withDisplayLength(10);
}
    ])