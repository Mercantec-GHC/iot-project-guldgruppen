import React from 'react'

function SmsTest() {

    const sendSms = async () => {
        await fetch('/api/sms/send', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({
                to: '+4555210815',
                message: 'Hello from Twilio!',
            }),
        });
    };

    return (
        <div>
            <h1>Test SMS</h1>
            <p>This is a test for sending SMS.</p>
            <button onClick={sendSms}>Send Test SMS</button>
        </div>
    );
}


export default SmsTest
