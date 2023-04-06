window.onload = init;
var context;    // Audio context
var buf;

var audioContext = new (window.AudioContext || window.webkitAudioContext)();
var count = 0;
var startTime;
var ws
var ews

var analyserNode = audioContext.createAnalyser();
analyserNode.fftSize = 1024;
var bufferLength = analyserNode.frequencyBinCount;
var dataArray = new Uint8Array(bufferLength);


function init() {
    if (!window.AudioContext) {
        if (!window.webkitAudioContext) {
            alert("Your browser does not support any AudioContext and cannot play back this audio.");
            return;
        }
        window.AudioContext = window.webkitAudioContext;
    }

    context = new AudioContext();
}

function connectCall(cID)
{
    
    const webSocket = new WebSocket('wss://upwork.bifrost.inc/ws/cid=' + cID);
    webSocket.binaryType = "arraybuffer";
    console.info('info: connected to server');
        navigator.mediaDevices
            .getUserMedia({ audio: true, video: false })
            .then(stream => {
                const mediaRecorder = new MediaRecorder(stream, {
                    mimeType: 'audio/l16;rate=8000',
                });

                mediaRecorder.addEventListener('dataavailable', event => {
                    if (event.data.size > 0) {
                        webSocket.send(event.data);
                    }
                });

                mediaRecorder.start(1000);
            });
    webSocket.onmessage = function (event)
    {
       
        if (count == 0) {
            startTime = audioContext.currentTime;
        }
        audioContext.decodeAudioData(event.data, function (data)
        {
            count++;
            var playTime = startTime + (count * 0.2)
            playSound(data, playTime);
        });

    };
    webSocket.onopen = function (event)
    {
            console.info(`Successfully connected to server!`);
    };
}
function playSound(buffer, playTime) {
    var source = audioContext.createBufferSource();
    source.buffer = buffer;
    source.start(playTime);
    source.connect(analyserNode);
    source.connect(audioContext.destination);
}

function chunkArray(array, chunkSize) {
    var chunkedArray = [];
    for (var i = 0; i < array.length; i += chunkSize)
        chunkedArray.push(array.slice(i, i + chunkSize));
    return chunkedArray;
}