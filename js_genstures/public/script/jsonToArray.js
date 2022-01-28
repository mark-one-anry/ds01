const IMAGE_WIDTH = 70;
const IMAGE_HEIGHT = 50;

function encode( s ) {
    var out = [];
    for ( var i = 0; i < s.length; i++ ) {
        out[i] = s.charCodeAt(i);
    }
    return new Uint8Array( out );
}

window.onload = function(){    
    let allImages = "";
    console.log("Got " + verificationData.length + " images");
    for(let img = 0; img<verificationData.length;img++){
        let imgArr = [];
        for(let j = 0; j<IMAGE_HEIGHT*IMAGE_WIDTH; j++ ){
            imgArr[j] = 0;
        }

        for(let c = 0; c < verificationData[img].length; c++){
            let x = Math.round(Math.round(verificationData[img][c].x));
            let y = Math.round(verificationData[img][c].y)*IMAGE_WIDTH;

            x = x <= IMAGE_WIDTH ? x : IMAGE_WIDTH;
            y = y <= (IMAGE_HEIGHT-1)*IMAGE_WIDTH ? y : (IMAGE_HEIGHT-1)*IMAGE_WIDTH;


            imgArr[y + x] = 1;
        }
        allImages+=imgArr.toString();
    }
    console.log(allImages);

    //var data = encode( allImages.toString());
    var data = encode( allImages);

    var blob = new Blob( [ data ], {
        type: 'application/octet-stream'
    });
    
    url = URL.createObjectURL( blob );
    var link = document.createElement( 'a' );
    link.setAttribute( 'href', url );
    link.setAttribute( 'download', 'convertedImages.txt' );
    
    var event = document.createEvent( 'MouseEvents' );
    event.initMouseEvent( 'click', true, true, window, 1, 0, 0, 0, 0, false, false, false, false, 0, null);
    link.dispatchEvent( event );

}