/*
 * Copyright AllSeen Alliance. All rights reserved.
 *
 *    Permission to use, copy, modify, and/or distribute this software for any
 *    purpose with or without fee is hereby granted, provided that the above
 *    copyright notice and this permission notice appear in all copies.
 *
 *    THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 *    WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 *    MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 *    ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 *    WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 *    ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 *    OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

package org.opentranslatorstothings.sample.hellotemperature;

import android.app.ProgressDialog;
import android.os.Bundle;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.Looper;
import android.os.Message;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.View;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import org.alljoyn.bus.BusAttachment;
import org.alljoyn.bus.BusException;
import org.alljoyn.bus.BusListener;
import org.alljoyn.bus.Mutable;
import org.alljoyn.bus.ProxyBusObject;
import org.alljoyn.bus.SessionListener;
import org.alljoyn.bus.SessionOpts;
import org.alljoyn.bus.Status;


public class MainActivity extends AppCompatActivity {

    /* Load the native alljoyn_java library. */

    static {
        System.loadLibrary("alljoyn_java");
    }

    private static enum UIMessagesEnum
    {
        TurnOn,
        TurnOff,
        SetBrightness
    }

    private static final int MESSAGE_GETDATA = 1;
    private static final int MESSAGE_UPDATE_TEMPERATURE = 2;
    private static final int MESSAGE_UPDATE_TREND = 3;
    private static final int MESSAGE_POST_TOAST = 5;
    private static final int MESSAGE_START_PROGRESS_DIALOG = 6;
    private static final int MESSAGE_STOP_PROGRESS_DIALOG = 7;
    private static final int MESSAGE_NOSENSORS = 8;

    private static final String LOG_TAG = "HelloTemperature";

    /* Handler used to make calls to AllJoyn methods. See onCreate(). */
    private BusHandler mBusHandler;

    private ProgressDialog mDialog;
    private TextView mTemperatureView;
    private TextView mTrendView;

    private Handler mUIHandler = new Handler() {
        @Override
        public void handleMessage(Message msg) {
            switch (msg.what) {
                case MESSAGE_GETDATA:
                    Toast.makeText(MainActivity.this,R.string.get_data_confirmation,Toast.LENGTH_SHORT).show();
                    break;
                case MESSAGE_NOSENSORS:
                    Toast.makeText(MainActivity.this,R.string.msg_nosensors,Toast.LENGTH_SHORT).show();
                    break;
                case MESSAGE_POST_TOAST:
                    Toast.makeText(getApplicationContext(), (String) msg.obj, Toast.LENGTH_LONG).show();
                    break;
                case MESSAGE_START_PROGRESS_DIALOG:
                    mDialog = ProgressDialog.show(MainActivity.this,
                            "",
                            "Searcing for sensors.\nPlease wait...",
                            true,
                            true);
                    break;
                case MESSAGE_STOP_PROGRESS_DIALOG:
                    mDialog.dismiss();
                    break;
                case MESSAGE_UPDATE_TEMPERATURE:
                    double temperature = (double) msg.obj;
                    mTemperatureView.setText(String.format("%1$4.0f",temperature) );
                    break;
                case MESSAGE_UPDATE_TREND:
                    double trend = (double) msg.obj;
                    mTrendView.setText(String.format("%1$4.1f",trend) );
                    break;
                default:
                    break;
            }
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);


        Button refreshButton = (Button) findViewById(R.id.refresh_button);
        refreshButton.setOnClickListener(
                new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        mBusHandler.sendEmptyMessage(BusHandler.GETDATA);
                        mUIHandler.sendEmptyMessage(MESSAGE_GETDATA);
                    }
                }
        );
        mTemperatureView = (TextView) findViewById(R.id.text_temperature);
        mTrendView = (TextView) findViewById(R.id.text_trend);

        /* Make all AllJoyn calls through a separate handler thread to prevent blocking the UI. */
        HandlerThread busThread = new HandlerThread("BusHandler");
        busThread.start();
        mBusHandler = new BusHandler(busThread.getLooper());

        /* Connect to an AllJoyn object. */
        mBusHandler.sendEmptyMessage(BusHandler.CONNECT);
        mUIHandler.sendEmptyMessage(MESSAGE_START_PROGRESS_DIALOG);

    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_main, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        int id = item.getItemId();

        //noinspection SimplifiableIfStatement
        if (id == R.id.action_settings) {
            return true;
        }

        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();

        /* Disconnect to prevent resource leaks. */
        mBusHandler.sendEmptyMessage(BusHandler.DISCONNECT);
    }

    /* This class will handle all AllJoyn calls. See onCreate(). */
    class BusHandler extends Handler {
        /*
         * Name used as the well-known name and the advertised name of the service this client is
         * interested in.  This name must be a unique name both to the bus and to the network as a
         * whole.
         *
         * The name uses reverse URL style of naming, and matches the name used by the service.
         */
        private static final String SERVICE_NAME = "org.OpenT2T.Sample.SuperPopular.TemperatureSensor";
        private static final String SERVICE_PATH = "/Service";
        private static final short CONTACT_PORT=42;

        private BusAttachment mBus;
        private ProxyBusObject mProxyObj;
        private HelloTemperatureSensorInterface mSensorInterface;

        private int     mSessionId;
        private boolean mIsInASession;
        private boolean mIsConnected;
        private boolean mIsStoppingDiscovery;

        /* These are the messages sent to the BusHandler from the UI. */
        public static final int CONNECT = 1;
        public static final int JOIN_SESSION = 2;
        public static final int DISCONNECT = 3;
        public static final int GETDATA = 4;

        public BusHandler(Looper looper) {
            super(looper);

            mIsInASession = false;
            mIsConnected = false;
            mIsStoppingDiscovery = false;
        }

        @Override
        public void handleMessage(Message msg) {
            switch(msg.what) {
            /* Connect to a remote instance of an object implementing the SimpleInterface. */
                case CONNECT: {
                    org.alljoyn.bus.alljoyn.DaemonInit.PrepareDaemon(getApplicationContext());
                /*
                 * All communication through AllJoyn begins with a BusAttachment.
                 *
                 * A BusAttachment needs a name. The actual name is unimportant except for internal
                 * security. As a default we use the class name as the name.
                 *
                 * By default AllJoyn does not allow communication between devices (i.e. bus to bus
                 * communication). The second argument must be set to Receive to allow communication
                 * between devices.
                 */
                    String packageName = getPackageName();
                    mBus = new BusAttachment(getPackageName(), BusAttachment.RemoteMessage.Receive);

                /*
                 * Create a bus listener class
                 */
                    mBus.registerBusListener(new BusListener() {
                        @Override
                        public void foundAdvertisedName(String name, short transport, String namePrefix) {
                            logInfo(String.format("MyBusListener.foundAdvertisedName(%s, 0x%04x, %s)", name, transport, namePrefix));
                        /*
                         * This client will only join the first service that it sees advertising
                         * the indicated well-known name.  If the program is already a member of
                         * a session (i.e. connected to a service) we will not attempt to join
                         * another session.
                         * It is possible to join multiple session however joining multiple
                         * sessions is not shown in this sample.
                         */
                            if(!mIsConnected) {
                                Message msg = obtainMessage(JOIN_SESSION);
                                msg.arg1 = transport;
                                msg.obj = name;
                                sendMessage(msg);
                            }
                        }
                    });

                /* To communicate with AllJoyn objects, we must connect the BusAttachment to the bus. */
                    Status status = mBus.connect();
                    logStatus("BusAttachment.connect()", status);
                    if (Status.OK != status) {
                        finish();
                        return;
                    }

                /*
                 * Now find an instance of the AllJoyn object we want to call.  We start by looking for
                 * a name, then connecting to the device that is advertising that name.
                 *
                 * In this case, we are looking for the well-known SERVICE_NAME.
                 */

                    status = mBus.findAdvertisedName(SERVICE_NAME);
                    logStatus(String.format("BusAttachement.findAdvertisedName(%s)", SERVICE_NAME), status);
                    if (Status.OK != status) {
                        finish();
                        return;
                    }


                    break;
                }
                case (JOIN_SESSION): {
                /*
                 * If discovery is currently being stopped don't join to any other sessions.
                 */
                    if (mIsStoppingDiscovery) {
                        break;
                    }

                /*
                 * In order to join the session, we need to provide the well-known
                 * contact port.  This is pre-arranged between both sides as part
                 * of the definition of the chat service.  As a result of joining
                 * the session, we get a session identifier which we must use to
                 * identify the created session communication channel whenever we
                 * talk to the remote side.
                 */
                    short contactPort = CONTACT_PORT;

                    SessionOpts sessionOpts = new SessionOpts();
                    sessionOpts.transports = (short)msg.arg1;
                    Mutable.IntegerValue sessionId = new Mutable.IntegerValue();

                    Status status = mBus.joinSession((String) msg.obj, contactPort, sessionId, sessionOpts, new SessionListener() {
                        @Override
                        public void sessionLost(int sessionId, int reason) {
                            mIsConnected = false;
                            logInfo(String.format("MyBusListener.sessionLost(sessionId = %d, reason = %d)", sessionId,reason));
                            mUIHandler.sendEmptyMessage(MESSAGE_START_PROGRESS_DIALOG);
                        }
                    });
                    logStatus("BusAttachment.joinSession() - sessionId: " + sessionId.value, status);

                    if (status == Status.OK) {
                    /*
                     * To communicate with an AllJoyn object, we create a ProxyBusObject.
                     * A ProxyBusObject is composed of a name, path, sessionID and interfaces.
                     *
                     * This ProxyBusObject is located at the well-known SERVICE_NAME, under path
                     * "/SimpleService", uses sessionID of CONTACT_PORT, and implements the SimpleInterface.
                     */
                        mProxyObj =  mBus.getProxyBusObject(SERVICE_NAME,
                                SERVICE_PATH,
                                sessionId.value,
                                new Class<?>[] { HelloTemperatureSensorInterface.class });

                    /* We make calls to the methods of the AllJoyn object through one of its interfaces. */
                        mSensorInterface =  mProxyObj.getInterface(HelloTemperatureSensorInterface.class);

                        mSessionId = sessionId.value;
                        mIsConnected = true;
                        mUIHandler.sendEmptyMessage(MESSAGE_STOP_PROGRESS_DIALOG);
                    }
                    break;
                }

            /* Release all resources acquired in the connect. */
                case DISCONNECT: {
                    mIsStoppingDiscovery = true;
                    if (mIsConnected) {
                        Status status = mBus.leaveSession(mSessionId);
                        logStatus("BusAttachment.leaveSession()", status);
                    }
                    mBus.disconnect();
                    getLooper().quit();
                    break;
                }

            /*
             * Call the service's TurnOn method through the ProxyBusObject.
             *
             * This will also print the String that was sent to the service and the String that was
             * received from the service to the user interface.
             */
                case GETDATA: {
                    try {
                        if (mSensorInterface != null) {
                            mUIHandler.sendEmptyMessage(MESSAGE_GETDATA);
                            double temperature = mSensorInterface.getCurrentTemperature();
                            double temperatureTrend = mSensorInterface.getTemperatureTrend();
                            sendUiMessage(MESSAGE_UPDATE_TEMPERATURE,temperature);
                            sendUiMessage(MESSAGE_UPDATE_TREND,temperatureTrend);
                        } else {
                            mUIHandler.sendEmptyMessage(MESSAGE_NOSENSORS);
                        }
                    }
                    catch (BusException ex) {
                        logException("HelloSensorsInterface.open()", ex);
                    }
                    break;
                }
                default:
                    break;
            }
        }

        /* Helper function to send a message to the UI thread. */
        private void sendUiMessage(int what, Object obj) {
            mUIHandler.sendMessage(mUIHandler.obtainMessage(what, obj));
        }
    }

    private void logStatus(String msg, Status status) {
        String log = String.format("%s: %s", msg, status);
        if (status == Status.OK) {
            Log.i(LOG_TAG, log);
        } else {
            Message toastMsg = mUIHandler.obtainMessage(MESSAGE_POST_TOAST, log);
            mUIHandler.sendMessage(toastMsg);
            Log.e(LOG_TAG, log);
        }
    }

    private void logException(String msg, BusException ex) {
        String log = String.format("%s: %s", msg, ex);
        Message toastMsg = mUIHandler.obtainMessage(MESSAGE_POST_TOAST, log);
        mUIHandler.sendMessage(toastMsg);
        Log.e(LOG_TAG, log, ex);
    }

    /*
     * print the status or result to the Android log. If the result is the expected
     * result only print it to the log.  Otherwise print it to the error log and
     * Sent a Toast to the users screen.
     */
    private void logInfo(String msg) {
        Log.i(LOG_TAG, msg);
    }
}
