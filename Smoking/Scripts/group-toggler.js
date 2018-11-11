$().ready(function() {
    $('.group-name').each(function() {
        var state = $.cookie('g' + $(this).attr('gid'));
        if (state == null)
            state = '0';
        $(this).attr('state', state);
    });
    $('.group-name').each(function () {
        if ($(this).attr('state') == '1')
            $(this).next('.group-content').show();
        else $(this).next('.group-content').hide();
    });

    $('.group-name').click(function() {
        var state = $(this).attr('state');
        state = (state == '1' ? '0' : '1');
        $(this).attr('state', state);
        $.cookie('g' + $(this).attr('gid'), state, { expires: 365 });
        if (state == '1')
            $(this).next('.group-content').fadeTo(300, 1);
        else {
            
            $(this).next('.group-content').fadeTo(300, 0);
            $(this).next('.group-content').hide();
        }
        return false;
    });
})