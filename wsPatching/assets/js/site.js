// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function initValidation() {
    jQuery('.js-validation').validate({
        ignore: [],
        errorClass: 'invalid-feedback animated fadeIn text-right',
        errorElement: 'div',
        errorPlacement: function (error, el) {
            jQuery(el).addClass('is-invalid');
            jQuery(el).parents('.form-group').append(error);
        },
        highlight: function (el) {
            jQuery(el).parents('.form-group').find('.is-invalid').removeClass('is-invalid').addClass('is-invalid');
        },
        success: function (el) {
            jQuery(el).parents('.form-group').find('.is-invalid').removeClass('is-invalid');
            jQuery(el).remove();
        }
    });


}
function initValidationHorizontal() {
    jQuery('.js-validation').validate({
        ignore: [],
        errorClass: 'invalid-feedback animated fadeIn text-right offset-sm-3 col-sm-7',
        errorElement: 'div',
        errorPlacement: function (error, el) {
            jQuery(el).addClass('is-invalid');
            jQuery(el).parents('.form-group').append(error);
        },
        highlight: function (el) {
            jQuery(el).parents('.form-group').find('.is-invalid').removeClass('is-invalid').addClass('is-invalid');
        },
        success: function (el) {
            jQuery(el).parents('.form-group').find('.is-invalid').removeClass('is-invalid');
            jQuery(el).remove();
        }
    });

}


function ClosePopupEditor() {
    $('#globalContentModal').modal('hide');
}

function ShowPopUpDelete(viewName, controller, objectID, objectName, reloadViewName, UseId, reloadObjectID) {
    swal({
        title: 'Are you sure?',
        text: 'Do you really want to delete <b> ' + objectName + '</b>?',
        type: 'question',
        showCancelButton: true,
        confirmButtonClass: 'btn btn-outline-danger m-1',
        cancelButtonClass: 'btn btn-outline-secondary m-1',
        confirmButtonText: 'Yes, delete it!',
        html: 'Do you really want to delete <br><br><b> ' + objectName + '</b>?<br><br>',
        preConfirm: function (e) {
            return new Promise(function (resolve) {
                setTimeout(function () {
                    resolve();
                }, 50);
            });
        }
    }).then(function (result) {
        //debugger;
        if (UseId == true) {
            var pData = { id: objectID, ParentId: reloadObjectID };
        }
        else {
            var pData = { id: objectID };
        }

        if (result.value) {
            var url = getActionURL(viewName, controller);
            $.ajax({
                url: url,
                type: 'POST',
                data: pData,
                success: function (result) {
                    if (result.failure) {
                        ShowPopUpUserError(result.message);
                    }
                    else if (result === false) { //default message if we havent defined the failure reason

                        ShowPopUpUserError('Something went wrong when attempting to delete. Please contact the site admin for assistance.');
                    }
                    else {
                        LoadPageBody(reloadViewName, controller, UseId, reloadObjectID);
                        ShowPopUpSuccessWithTimer('Deleted!', '<b>' + objectName + '</b> has been deleted.');
                        //swal('Deleted!', '<b>' + objectName + '</b> has been deleted.', 'success');

                        
                    }
                },
                failure: function (result) {
                }
            });

            // result.dismiss can be 'overlay', 'cancel', 'close', 'esc', 'timer'
        } else if (result.dismiss === 'cancel') {
            //swal('Cancelled', 'Your imaginary file is safe :)', 'error');
        }
    });
}

function ShowPopUpDeleteDynamic(viewName, controller, objectID, objectName, reloadViewName, reloadControllerName, UseId, reloadObjectID, reloadEntireView) {
    swal({
        title: 'Are you sure?',
        text: 'Do you really want to delete <b> ' + objectName + '</b>?',
        type: 'question',
        showCancelButton: true,
        confirmButtonClass: 'btn btn-outline-danger m-1',
        cancelButtonClass: 'btn btn-outline-secondary m-1',
        confirmButtonText: 'Yes, delete it!',
        html: 'Do you really want to delete <br><br><b> ' + objectName + '</b>?<br><br>',
        preConfirm: function (e) {
            return new Promise(function (resolve) {
                setTimeout(function () {
                    resolve();
                }, 50);
            });
        }
    }).then(function (result) {
        //debugger;
        if (UseId == true) {
            var pData = { id: objectID, ParentId: reloadObjectID };
        }
        else {
            var pData = { id: objectID };
        }

        if (result.value) {
            var url = getActionURL(viewName, controller);
            $.ajax({
                url: url,
                type: 'POST',
                data: pData,
                success: function (result) {
                    if (result.failure) {
                        ShowPopUpUserError(result.message);
                    }
                    else if (result === false) { //default message if we havent defined the failure reason

                        ShowPopUpUserError('Something went wrong when attempting to delete. Please contact the site admin for assistance.');
                    }
                    else {
                        if (reloadEntireView) {
                            //full view reload
                            var reloadUrl = getActionURL(reloadViewName, reloadControllerName);
                            if (UseId) {
                                reloadUrl = reloadUrl + "/" + reloadObjectID;
                            }
                            window.location = reloadUrl;
                        }
                        else {
                            LoadPageBody(reloadViewName, reloadControllerName, UseId, reloadObjectID);
                        }
                        ShowPopUpSuccessWithTimer('Deleted!', '<b>' + objectName + '</b> has been deleted.');
                        //swal('Deleted!', '<b>' + objectName + '</b> has been deleted.', 'success');


                    }
                },
                failure: function (result) {
                }
            });

            // result.dismiss can be 'overlay', 'cancel', 'close', 'esc', 'timer'
        } else if (result.dismiss === 'cancel') {
            //swal('Cancelled', 'Your imaginary file is safe :)', 'error');
        }
    });
}

function ShowPopUpUserError(htmlMessage) {
    swal({
        title: 'Failed!',
        type: 'error',
        html: "<b>" + htmlMessage + "</b>"
    });
}


function ShowPopUpEditor(viewName, controller, objectID, objectType, parentObjectID, saveMethod) {
    //get modal object from DOM
    modal = $('#globalContentModal');

    $('#contentModalTitleText').text(objectType);

    $('#saveBtnModelContent').attr("onclick", saveMethod)

    var url = getActionURL(viewName, controller);
    url = url + "/" + objectID + '?ParentID=' + parentObjectID;

    $('#contentModalContent').load(url);


    $(modal).modal({
        backdrop: 'static',
        keyboard: false
    });



    $(modal).modal('show');
}

function ShowPopUpEditorDynamic(viewName, controller, objectID, objectType, ProcessMethod, ProcessController, ReloadAction, ReloadController, useId, Id) {
    //get modal object from DOM
    modal = $('#globalContentModal');

    $('#contentModalTitleText').text(objectType);

    //debugger;
    $('#saveBtnModelContent').attr("onclick", "ProcessForm('" + ProcessMethod + "','" + ProcessController + "','" + ReloadAction + "','" + ReloadController + "','" + useId + "','" + Id + "')");
    var url = getActionURL(viewName, controller);
    url = url + "/" + objectID
    if (useId) {
        url = url + "?ParentId=" + Id;
    }

    $('#contentModalContent').load(url);

   
    $(modal).modal({
        backdrop: 'static',
        keyboard: false
    });


    
    $(modal).modal('show');
}

function ShowPopUpEditorDynamicWithViewReload(viewName, controller, objectID, objectType, ProcessMethod, ProcessController, ReloadAction, ReloadController, useId, Id, reloadEntireView) {
    //get modal object from DOM
    modal = $('#globalContentModal');

    $('#contentModalTitleText').text(objectType);

    //debugger;
    $('#saveBtnModelContent').attr("onclick", "ProcessFormDynamicWithViewReload('" + ProcessMethod + "','" + ProcessController + "','" + ReloadAction + "','" + ReloadController + "','" + useId + "','" + Id + "','" + reloadEntireView + "')");
    var url = getActionURL(viewName, controller);
    url = url + "/" + objectID
    if (useId) {
        url = url + "?ParentId=" + Id;
    }

    $('#contentModalContent').load(url);


    $(modal).modal({
        backdrop: 'static',
        keyboard: false
    });



    $(modal).modal('show');
}

function ProcessForm(Action, Controller, ReloadAction, ReloadController, UseId, Id) {
    
    if ($("#frmEditor").valid()) {

        var url = getActionURL(Action, Controller);
        var fData = $("#frmEditor").serialize();

        $("#frmEditor").submit(function (e) {
            e.preventDefault();
        });


        $.ajax({

            type: 'POST',
            url: url,
            data: fData,
            success: function (result) {
                //debugger;
                if (!result.failure) {
                    ClosePopupEditor();
                    
                    if (UseId) {
                        if (Id == 'new') {
                            var newId = result.id;
                            LoadPageBody(ReloadAction, ReloadController, true, newId);
                        }
                        else {
                            LoadPageBody(ReloadAction, ReloadController, UseId, Id);
                        }

                    }
                    else {
                        LoadPageBody(ReloadAction, ReloadController, false, Id);
                    }
                    ShowPopUpSuccessWithTimer("Success!", "Your changes have been saved.");
                }
                else {
                    //swal.close();
                    ShowPopUpUserError(result.message);
                }
                
                
            },
            fail: function (data) {
                alert("fail");
            }
        });
    }

}

function ProcessFormDynamicWithViewReload(Action, Controller, ReloadAction, ReloadController, UseId, Id, reloadEntireView) {

    if ($("#frmEditor").valid()) {

        var url = getActionURL(Action, Controller);
        var fData = $("#frmEditor").serialize();

        $("#frmEditor").submit(function (e) {
            e.preventDefault();
        });


        $.ajax({

            type: 'POST',
            url: url,
            data: fData,
            success: function (result) {
                //debugger;
                if (!result.failure) {
                    ClosePopupEditor();

                    if (UseId) {
                        if (Id == 'new') {
                            var newId = result.id;
                            if (reloadEntireView) {
                                var reloadUrl = getActionURL(ReloadAction, ReloadController) + "/" + newId;
                                window.location = reloadUrl;
                            }
                            else {
                                LoadPageBody(ReloadAction, ReloadController, true, newId);
                            }
                        }
                        else {
                            if (reloadEntireView) {
                                var reloadUrl = getActionURL(ReloadAction, ReloadController) + "/" + Id;
                                window.location = reloadUrl;
                            }
                            else {
                                LoadPageBody(ReloadAction, ReloadController, UseId, Id);
                            }
                        }

                    }
                    else {
                        if (reloadEntireView) {
                            var reloadUrl = getActionURL(ReloadAction, ReloadController);
                            window.location = reloadUrl;
                        }
                        else {
                            LoadPageBody(ReloadAction, ReloadController, false, Id);
                        }
                    }
                    ShowPopUpSuccessWithTimer("Success!", "Your changes have been saved.");
                }
                else {
                    //swal.close();
                    ShowPopUpUserError(result.message);
                }


            },
            fail: function (data) {
                alert("fail");
            }
        });
    }

}

function ShowPopUpSuccessWithTimer(title, htmlMessage) {
    Swal.fire({
        //position: 'top-end',
        type: 'success',
        title: title,
        html: "<b>" + htmlMessage + "</b",
        showConfirmButton: false,
        timer: 2000
    });
}

function LoadPageBody(LoadAction, LoadController, UseId, Id) {



    var url = getActionURL(LoadAction, LoadController);
    if (UseId) {
        url = url + "/" + Id;
    }
    

    $("#pageBody").load(url, function () {
        //this is one sure fire way to always ensure datatables are loaded in partialviews. need to find out why scripts on page aren't working.
        //$('#detailsTable').dataTable({
        //    "aaSorting": [],
        //    "pageLength": 10
        //});
    });

    $("html, body").animate({ scrollTop: 0 }, "slow");


}

function LoadGlobalListsOriginal() {

    $.ajax({

        type: "GET",

        async: false,

        url: getActionURL("GetApplicationList", "Global"),

        dataType: "json",

        success: function (obj) {

            AllAppList = obj;

        }

    });



    $.ajax({

        type: "GET",

        async: false,

        url: getActionURL("GetServerList", "Global"),

        dataType: "json",

        success: function (obj) {

            AllServerList = obj;

        }

    });

}



function LoadGlobalLists() {

    $.ajax({

        type: "GET",

        async: false,

        cache: true,

        url: getActionURL("GetGlobalSearchList", "Global"),

        dataType: "json",

        success: function (obj) {

            AllAppList = obj.apps;

            AllServerList = obj.servers;



            $('#globalSearchType').change(function () {

                var obj;

                localStorage.setItem("globalST", $(this).val());

                if ($(this).val() === "application") {

                    obj = AllAppList;

                }

                else {

                    obj = AllServerList;

                }

                InitLargeSelect2DDL(obj, 'globalSearchSelect', 50);

            });



            $('#globalSearchTypeSmall').change(function () {



                localStorage.setItem("globalST", $(this).val());

                var obj;

                if ($(this).val() === "application") {

                    obj = AllAppList;

                }

                else {

                    obj = AllServerList;

                }

                InitLargeSelect2DDL(obj, 'globalSearchSelectSmall', 50);

            });



            var globalST = localStorage.getItem("globalST");

            if (globalST) {

                $('#globalSearchType').val(globalST).change();

                $('#globalSearchTypeSmall').val(globalST).change();

            }



            $("#globalSearch").fadeIn('fast');

            $("#globalSearchSmall").fadeIn('fast');



        }

    });




}





//initialize select2 element with large number of items

//list = item with value and label property, select2element = id of element to initialize, pageSize = default # of results to show

function InitLargeSelect2DDL(list, select2element, pageSize) {


    
    items = [];

    for (var i = 0; i < list.length; i++) {

        items.push({ id: list[i].value, text: list[i].text });

    }
    //debugger;


    jQuery.fn.select2.amd.require(["select2/data/array", "select2/utils"],



        function (ArrayData, Utils) {
            //debugger;
            function CustomData($element, options) {

                CustomData.__super__.constructor.call(this, $element, options);

            }

            Utils.Extend(CustomData, ArrayData);



            CustomData.prototype.query = function (params, callback) {



                results = [];

                if (params.term && params.term !== '') {

                    results = _.filter(items, function (e) {

                        return e.text.toUpperCase().indexOf($.trim(params.term.toUpperCase())) >= 0;

                    });

                } else {

                    results = items;

                }



                if (!("page" in params)) {

                    params.page = 1;

                }

                var data = {};

                data.results = results.slice((params.page - 1) * pageSize, params.page * pageSize);

                data.pagination = {};

                data.pagination.more = params.page * pageSize < results.length;

                callback(data);

            };

            if ($('#' + select2element).data('select2')) {

                $('#' + select2element).select2('destroy');

            }

            $('#' + select2element).empty().select2({

                ajax: {},

                dataAdapter: CustomData

            });

        });

}





function InitScriptEditorFromController(fieldName, qaScriptId, readOnly) {
    require.config({ paths: { 'vs': '../assets-external/Monaco/vs' } });
    var data = { id: qaScriptId };
    var url = getActionURL("_GetQaChecklistScript", "Report");
    var jqxhr = $.post(url, data)
        .done(function (result) {
            var qaScript = result.script;
            require(['vs/editor/editor.main'], function () {
                window.editor = monaco.editor.create(document.getElementById(fieldName), {
                    value: qaScript,
                    language: 'powershell',
                    theme: 'vs-dark',
                    minimap: {
                        enabled: false
                    },
                    readOnly: readOnly,
                    automaticLayout: true,
                    scrollBeyondLastLine: false

                });
            });
        })
}


