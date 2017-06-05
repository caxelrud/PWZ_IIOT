import json
import sys
import getopt
#from getopt import getopt

# var config = { splitby: 'keyname1[.]keyname2[.]keyname3[.]' }
#var config = {splitby:'root'}
#var config = {splitby:'root[.]a1'}
#var config = {splitby:'root[.]a1[.]a2'}
#var config =  {splitby:'root[.]a1[.]'}

config = {'splitby':''}
msgToSend = []

def processMsg(param_msg):
    arraykeys = str(config['splitby']).split('[.]')
    print(str(len(arraykeys)))
    #msg = [{},{}]
    msg = json.loads(param_msg)
    if (len(arraykeys) == 1 or len(arraykeys[0]) <=0):
        if isinstance(msg,list):
            print('yes its an array')
            for val in msg:
                for i in val.keys():
                    print(i)
                for k in val.values():
                    print(k)
                #msgToSend.insert(val)
    if(len(arraykeys)==1 and len(arraykeys[0])>0):
        if isinstance(msg,list):
            for val in msg:
                print(val)

                #if val == arraykeys[0]:

def main(argv):
    #try:
    opts, args = getopt.getopt(argv, 'm',['message'])
    #except getopt.GetoptError:
       # sys.exit(2)
    for opt, arg in opts:
        if opt in ('-m','--message'):
            print('areg = ' + arg)
            processMsg(arg)


if __name__ == "__main__":
    #main(sys.argv[1:])
    msg = '[{"element1":"value1","element2":"value2","element3":"value3"},{"element1":"value10","element2":"value20","element3":"value30"},{"element1":"value11","element2":"value21","element3":"value31"}]'
    processMsg(msg)
