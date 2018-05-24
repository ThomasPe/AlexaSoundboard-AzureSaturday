const request = require('request');
const cheerio = require('cheerio');
const soundbaseuri = 'https://www.myinstants.com/media/sounds/';
module.exports = function (context, myQueueItem) {
    context.log('JavaScript queue trigger function processed work item:', myQueueItem);
    var query = myQueueItem.split(' ').join('+');
    var name = myQueueItem.split(' ').join();
    request('https://www.myinstants.com/search/?name=' + query, function (error, response, html) {
        if (!error && response.statusCode == 200) {
            context.log("successful download of html");
            var $ = cheerio.load(html);
            var sound = $('.instant .small-button').first().attr('onmousedown').replace("play('/media/sounds/", "").replace("')", "");
            context.log(sound);
            context.binding.soundUri = soundbaseuri + sound + ';' + name;
            context.done();
        } else {
            context.log("error: " + error);
            context.done();
        }
    });

};