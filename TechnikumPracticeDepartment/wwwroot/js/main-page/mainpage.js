    var codropsEvents = {
        '11-21-2013': '<a href="http://www.wincalendar.com/Great-American-Smokeout">Great American Smokeout</a>',
        '11-13-2013': '<span>Ashura [An example of an complete date event (11-13-2013)]</span>',
        '11-11-2013': '<a href="http://www.wincalendar.com/Remembrance-Day">Remembrance Day (Canada)</a>',
        '11-04-2013': '<span>Islamic New Year</span>',
        '11-03-2013': '<a href="http://www.wincalendar.com/Daylight-Saving-Time-Ends">Daylight Saving Time Ends</a>',
        '11-01-2013': '<span>All Saints Day</span>',
        '12-25-YYYY': '<span>Christmas Day [Date : 12-25-YYYY]</span>',
        '01-01-YYYY': '<span>New Year\'s Eve (Event repeats every YEAR)</span>',
        'MM-02-2013': '<span>Yeah, Monthly [MM-02-2013]</span>',
        'MM-07-YYYY': '<span>[MM-07-YYYY], Monthly and Yearly</span>',
        '11-DD-2014': { content: '<span>Ex: {\'11-DD-2014\' : {content : \'Something\', endDate : 20}}</span>', endDate: 20 },
        '02-DD-2014': { content: '<span>Ex: {\'02-DD-2014\' : {content : \'Something\', startDate : 10, endDate : 20}}</span>', startDate: 10, endDate: 20 },
        '12-DD-YYYY': '<span>[12-DD-YYYY] Whole month and Year</span>',
        'TODAY': '<span>Today, [Date : \'TODAY\']</span>',
        '10-16-2014': ['<a href="">event one</a>', '<span>event two</span>'],
        '10-DD-YYYY': [
            { content: '<span>Ex: {\'10-DD-2014\' : {content : \'Something\', startDate : 10, endDate : 20}}</span>', startDate: 10, endDate: 20 },
            { content: '<span>Ex: {\'10-DD-2014\' : {content : \'Something\', endDate : 20}}</span>', endDate: 20 },
        ]

    };
    $(function() {
        $(document).on('shown.calendar.calendario', function(e, instance) {
            if (!instance) instance = cal;
            var $cell = instance.getCell(new Date().getDate());
        });

        var transEndEventNames = {
                'WebkitTransition': 'webkitTransitionEnd',
                'MozTransition': 'transitionend',
                'OTransition': 'oTransitionEnd',
                'msTransition': 'MSTransitionEnd',
                'transition': 'transitionend'
            },
            transEndEventName = transEndEventNames[Modernizr.prefixed('transition')],
            $wrapper = $('#custom-inner'),
            $calendar = $('#calendar'),
            cal = $calendar.calendario({
                onDayClick: function($el, data, dateProperties) {

                    if (data.content.length > 0) {
                        showEvents(data.content, dateProperties);
                    }

                },
                caldata: codropsEvents,
                displayWeekAbbr: true,
                events: 'click'
            }),
            $month = $('#custom-month').html(cal.getMonthName()),
            $year = $('#custom-year').html(cal.getYear());

        $('#custom-next').on('click', function() {
            cal.gotoNextMonth(updateMonthYear);
        });
        $('#custom-prev').on('click', function() {
            cal.gotoPreviousMonth(updateMonthYear);
        });

        function updateMonthYear() {
            $month.html(cal.getMonthName());
            $year.html(cal.getYear());
        }

        // just an example..
        function showEvents(contentEl, dateProperties) {

            hideEvents();

            var $events = $('<div id="custom-content-reveal" class="custom-content-reveal"><h4>События ' + dateProperties.monthname + ' ' + dateProperties.day + ', ' + dateProperties.year + '</h4></div>'),
                $close = $('<span class="custom-content-close"></span>').on('click', hideEvents);

            $events.append(contentEl.join(''), $close).insertAfter($wrapper);

            setTimeout(function() {
                $events.css('top', '0%');
            }, 25);

        }

        function hideEvents() {

            var $events = $('#custom-content-reveal');
            if ($events.length > 0) {

                $events.css('top', '100%');
                Modernizr.csstransitions ? $events.on(transEndEventName, function() {
                    $(this).remove();
                }) : $events.remove();

            }

        }

    });