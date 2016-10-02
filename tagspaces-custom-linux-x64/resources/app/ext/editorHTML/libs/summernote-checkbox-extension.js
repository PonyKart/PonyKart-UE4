(function (factory) {
    /* global define */
    if (typeof define === 'function' && define.amd) {
        // AMD. Register as an anonymous module.
        define(['jquery'], factory);
    } else if (typeof module === 'object' && module.exports) {
        // Node/CommonJS
        module.exports = factory(require('jquery'));
    } else {
        // Browser globals
        factory(window.jQuery);
    }
}(function ($) {
    // Extends plugins for adding Checkbox.
    $.extend($.summernote.plugins, {
        /**
         * @param {Object} context - context object has status of editor.
         */
        'checkbox': function (context) {
            var self = this;
            var ui = $.summernote.ui;

            context.memo('button.checkbox', function () {
                var button = ui.button({
                    contents: '<i class="fa fa-check"/>',
                    tooltip: 'Add Checkbox / ToDo',
                    click: function () {
                        context.invoke('insertNode', self.createCheckbox());
                    }
                });

                return button.render();
            });

            this.createCheckbox = function () {
                var elem = document.createElement('input');
                elem.type = "checkbox";
                elem.className = "tsCheckBox";
                return elem;
            }

            // This events will be attached when editor is initialized.
            this.events = {
                'summernote.init': function (we, e) {                    
                },
                'summernote.keyup': function (we, e) {                    
                }
            };

            this.initialize = function () {
                var layoutInfo = context.layoutInfo;
                var $editor = layoutInfo.editor;

                $editor.click(function (event) {
                    if (event.target.type && event.target.type == 'checkbox') {
                        var checked = $(event.target).is(':checked');
                        $(event.target).attr('checked', checked);
                        context.invoke('insertText', '');
                    }
                });
            };

            this.destroy = function () {

            };
        }
    });
}));