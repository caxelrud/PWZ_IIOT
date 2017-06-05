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
function processArray(obj, arraykeys, keyIndex)
{
   
    Object.keys(obj).forEach(function (key) {
        if (arraykeys[keyIndex] == key) {
            if(keyIndex < arraykeys.length){
               keyIndex++;
              }            
            if (Array.isArray(obj[key])) {
                var i = 0;
                while (i < obj[key].length) {
                    if (Array.isArray(obj[key][i])) {
                       
                        processArray(obj[key][i], arraykeys, keyIndex);
                    }
                    else {
                        msgToSend.push(obj[key][i]);
                    }
                    i++;
                }
            }
                    
            }
            
            
            
        
    });
}

function processMsg(msg)
{
    console.log("message msg "+ msg);
    var obj = JSON.parse(msg);
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
 /*       Object.keys(obj).forEach(function (key) {
            console.log("processing field " + obj[key]);
            if (arraykeys[arrIndex] == key) {
                console.log("found first key " + key + "arrIndex : " + arrIndex);
                arrIndex++;
                   if (Array.isArray(obj[key])) {
                    console.log("obj[key] is an array " + key);
                    var i = 0;
                    while (i < obj[key].length)
                    {
                        console.log("obj[key] length for i " + i + obj[key].length);
                        if (Array.isArray(obj[key][i])) {
                            console.log("Sending following to process array" + JSON.stringify(obj[key][i]) + " arraykeys " + arraykeys + "array index "+ arrIndex);  
                            processArray(obj[key][i], arraykeys, arrIndex);
                        }
                        else {
                            msgToSend.push(obj[key][i]);
                        }
                        i++;
                   }
                
                  }
                  else
                
            }
        });*/
    }

}
var fs = require('fs');
// Provide the hierarcy by which you would like to split the array. For each keyname it will assume as an array
// following are some of the sample configs
// Split by Unnamed root array: [{"element1":"value1","element2":"value2"},{"element11":"value11","element21":"value21"},{"element12":"value12","element22":"value22"}]  the config will be {splitby:''
// Split by named root array:i {"root":[{"e1":"v1","e2":"v2"},{"e1":"v11","e2":"v21"},{"e1":"v12","e2":"v22"}]} the config would be {splitby:'root'}
// Array: [{"r":[{"a1":[{"e1":"v1},{"e2":"v2"}]},{"a1":[{"e1":"v11"},{"e2:"v21""}]},{"a1":[{"e1":"v12"},{"e2:"v22""}]}]}] the config would be {splitby:'r[.]a1'}
// Array: [{"r":[{"a1":[{"a2":[{"e1":"v1","e2":"v2"}]} ,{"a1":[{"e1":"v11","e21":"v21"}] },{"a1":[{"e1":"v12","e21":"v22"}] } }] the config would be {splitby:'r[.]a1[.]a2'}
// Array: [{"r":[{"a1":[{"e1":"v1","e2":"v2"},{"e1":"v11","e2":"v21"},{"e1":"v12","e2":"v22"} ]}]} the config would be {splitby:'r[.]a1[.]'}

//var config = { splitby: 'keyname1[.]keyname2[.]keyname3[.]' }
//var config = {splitby:'root'}
//var config = {splitby:'root[.]a1'}
//var config = {splitby:'root[.]a1[.]a2'}
var config = {splitby:'root[.]a1[.]'}
console.log("parsing array sample 3");
// Modify this part to read from IoT Hub
//var file1 = fs.readFileSync('sample1.json');
var file1 = fs.readFileSync('sample4.json');
//var file1 = fs.readFileSync('sample3.json');

var msgToSend = []

processMsg(file1);

var x = 0;
while (x < msgToSend.length)
{
    console.log("printing message to send...");
    console.log(msgToSend[x]);
    x++;
}




/*
arraykeys.forEach(function (key) {
    console.log("key " + ":" + key);
});
*/
/*
console.log("object array is array " + Array.isArray(obj));

console.log("object length " + obj.length);
var i = 0;
while (i < obj.length) {
    Object.keys(obj[i]).forEach(function (key) {
        var value = obj[i][key];
        console.log("Object " + i);
        console.log(key + ":" + value);
        // sending the message as individual message to an event hub

    });
    i++;
}*/

