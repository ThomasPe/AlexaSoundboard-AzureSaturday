const verifier = require('alexa-verifier');


module.exports = function (context, myQueueItem) {
    context.log('JavaScript queue trigger function processed work item:', myQueueItem);
    context.done();
};