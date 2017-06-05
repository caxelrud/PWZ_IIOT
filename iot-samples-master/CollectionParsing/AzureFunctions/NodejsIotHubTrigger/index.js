//var config = { splitby: 'keyname1[.]keyname2[.]keyname3[.]' }
//var config = {splitby:'root'}
//var config = {splitby:'root[.]a1'}
//var config = {splitby:'root[.]a1[.]a2'}
//var config =  {splitby:'root[.]a1[.]'}
var config = {splitby:''}
var msgToSend = []

module.exports = function (context, myEventHubTrigger) {
    context.log('Node.js eventhub trigger function processed work item', myEventHubTrigger);
    
// Modify this part to read from IoT Hub
//var file1 = fs.readFileSync('sample1.json');

//var file1 = fs.readFileSync('sample3.json');

//context.log(JSON.parse(my))

processMsg(myEventHubTrigger);
if(msgToSend.length>0)
{
    var i = 0;
    while(i<msgToSend.length)
    {
        context.bindings.outputEventHubMessage = JSON.stringify(msgToSend[i]);
        i++;
    }
    
    
}


    context.done();
};

function processObj(obj,arraykeys,keyIndex)
{
  
   if(keyIndex != arraykeys.length-1)
  {

    Object.keys(obj).forEach(function(key){
        if(key == arraykeys[keyIndex])
        {
              if(keyIndex < arraykeys.length)
              {keyIndex++; }
              if(Array.isArray(obj[key]))
              {
                  var k = 0;
                  while(k<obj[key].length)
                  {    
                       processObj(obj[key][k],arraykeys,keyIndex);   
                       k++;
                    
                  }
  
              }
              else
              {
                  
                  processObj(obj[key],arraykeys,keyIndex);
              }
        }    
          


    });
                    
   
  }
  else
 {
    if(arraykeys[keyIndex]=='')
    {
        if(Array.isArray(obj))
        {
           var k = 0;
           while(k<obj.length)
           {
              msgToSend.push(obj[k]);
              k++;
           }
        }
        else
        {
            msgToSend.push(obj);
        }    
    }
    else
    { 
     Object.keys(obj).forEach(function(key){

               if(key==arraykeys[arraykeys.length-1])
               {
                 
                      if(Array.isArray(obj[key]))
                      {
                         var k =0;
                         while(k<obj[key].length)
                         {
                               msgToSend.push(obj[key][k]);
                               k++;
                         } 
                      }
                      else
                      {
                         msgToSend.push(obj[key]);    
                      }   
 
               }                     
            


        }); }
          
 } 

}
function processMsg(msg)
{
    console.log("message msg "+ msg);
    //var obj = JSON.parse(msg);
    var obj =  msg;
    // ****//
    var arraykeys = config.splitby.split("[.]")

    console.log("arraykeys length : " + arraykeys.length);

    

    if (arraykeys.length == 1 && arraykeys[0].length <= 0) {
        if (Array.isArray(obj)) {
            var i = 0
            while (i < obj.length) {
                msgToSend.push(obj[i]);
                i++;

            }
        }
        else {
            console.log('JSON object does not match with the config provided');
        }
    }

    if (arraykeys.length == 1 && arraykeys[0].length > 0) {

        var i = 0;

        Object.keys(obj).forEach(function (key) {
            
            if (key == arraykeys[0]) {
                if (Array.isArray(obj[key])) {
                    while (i < obj[key].length) {
                        msgToSend.push(obj[key][0]);
                        i++;
                    }
                }
                else {
                    console.log("JSON object does not match with the config provided");
                }

            }
        });


    }
    if (arraykeys.length > 1) {

        var arrIndex = 0
        processObj(obj,arraykeys,arrIndex);

    }

}
