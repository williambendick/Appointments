
var CLIENT_ID = '';
var API_KEY = '';
var DISCOVERY_DOCS = ["https://www.googleapis.com/discovery/v1/apis/calendar/v3/rest"];
var SCOPES = "https://www.googleapis.com/auth/calendar";
var GoogleAuth;

async function makeCalendarAPIRequest(requestType, requestData) {
    
    var result;

    // load gapi library if it has not been loaded yet
    if (!GoogleAuth) {

        await gapi.load('client:auth2');

        result = await initializeGAPI();

        if (result != 'success') return formatErrorMessage('initializeGAPI', result);
    }

    // sign in user if they're not signed in
    if (!GoogleAuth.isSignedIn.get()) {

        result = await signInUser();

        if (result != 'success') return formatErrorMessage('signInUser', result);
    }

    // make API request
    switch (requestType) {

        case 'create':
            return await createEvent(requestData);

        case 'update':            
            return await updateEvent(requestData);

        case 'delete':
            return await deleteEvent(requestData);

        default:
            return 'a valid type was not included with request data.';
    }
}

function initializeGAPI() {
    return new Promise(function (resolve) {
        gapi.client.init({
            apiKey: API_KEY,
            clientId: CLIENT_ID,
            discoveryDocs: DISCOVERY_DOCS,
            scope: SCOPES
        }).then(function () {
            GoogleAuth = gapi.auth2.getAuthInstance();

            resolve('success');
           
        }, function (error) {
            resolve(error);
        });
    });
}

function signInUser() {
    return new Promise(function (resolve) {
        GoogleAuth.signIn()
            .then(function () {
                resolve('success');
            }, function (error) {
                resolve(error);
            });
    });
}

function makeRequest(apiRequest) {
    return new Promise(resolve => {
        apiRequest.execute(response => resolve(response));
    });
}

async function createEvent(event) {

    var request = gapi.client.calendar.events.insert({
        'calendarId': 'primary',
        'resource': event
    });

    var result = await makeRequest(request);

    if (result.id)
        return 'Event created with id ' + result.id;
    else
        return formatErrorMessage('makeRequest', result);  
}

async function updateEvent(event) {

    var request = gapi.client.calendar.events.patch({
        'calendarId': 'primary',
        'eventId': event.id,
        'resource': event
    });

    var result = await makeRequest(request);

    if (result.error)
        return formatErrorMessage('makeRequest', result);  
    else
        return 'Event with id ' + result.id + ' was updated'
}

async function deleteEvent(event) {

    var request = gapi.client.calendar.events.delete({
        'calendarId': 'primary',
        'eventId': event.id,
        'resource': event
    });

    var result = await makeRequest(request);

    if (result.error)
        return formatErrorMessage('makeRequest', result);
    else
        return 'Event with id ' + event.id + ' was deleted'
}

function formatErrorMessage(errorType, errorData) {

    var message = 'An error occurred';

    if (errorType === 'initializeGAPI') {
        message += ' when initializing Google API library';
    }
    else if (errorType === 'signInUser') {
        message += ' when signing in user';
    } 
    else if (errorType === 'makeRequest') {
        message += ' when making API request';
    } 

    if (errorData) {

        if (errorData.error && typeof errorData.error === 'string') message += ': ' + errorData.error;

        if (errorData.details) message += ': ' + errorData.details;

        if (errorData.error && errorData.error.message) message += ': ' + errorData.error.message;
    }

    return message;
}
