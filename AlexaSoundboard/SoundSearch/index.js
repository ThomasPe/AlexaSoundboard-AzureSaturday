var request = require('request');
var cheerio = require('cheerio');

module.exports = function (context, myQueueItem) {
    context.log('JavaScript queue trigger function processed work item:', myQueueItem);

    request('https://www.myinstants.com/search/?name=power+rangers+theme', function (error, response, html) {
        if (!error && response.statusCode == 200) {
            var $ = cheerio.load(html);
            var sound = $('.instant .small-button').first().attr('onmousedown').replace("play('/media/sounds/", "").replace("')", "");
            console.log(sound);
        }
        context.done();
    });

};