// /Scripts/site.js
$(document).ready(function() {
    // Utility for active link persistence if needed
    
    // Smooth scroll for dashboard cards or similar
    
    // AJAX Global CSRF Protection via Prefilter
    $.ajaxPrefilter(function (options, originalOptions, jqXHR) {
        if (options.type.toUpperCase() === "POST") {
            var token = $('input[name="__RequestVerificationToken"]').val();
            if (token) {
                // Ensure data is an object
                if (typeof options.data === "string") {
                    if (options.data.indexOf("__RequestVerificationToken") === -1) {
                        options.data += (options.data.length > 0 ? "&" : "") + "__RequestVerificationToken=" + encodeURIComponent(token);
                    }
                } else {
                    options.data = $.extend(options.data || {}, {
                        "__RequestVerificationToken": token
                    });
                }
            }
        }
    });

    // AJAX Global error handler for enterprise robustness
    $(document).ajaxError(function(event, jqXHR, ajaxSettings, thrownError) {
        console.error("AJAX Error: " + thrownError);
        if (jqXHR.status === 403) {
            console.warn("CSRF Failure or Unauthorized. Token status: " + ($('input[name="__RequestVerificationToken"]').length > 0));
            // alert("Security token expired. Please refresh the page.");
        }
    });
});
