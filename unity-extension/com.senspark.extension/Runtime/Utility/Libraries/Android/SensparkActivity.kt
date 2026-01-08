package com.senspark.core

import android.content.Intent
import android.os.Bundle
import android.util.Log
import com.unity3d.player.UnityPlayerActivity
import com.senspark.core.internal.PluginManager

class SensparkActivity : UnityPlayerActivity() {

    private lateinit var _pluginManager: PluginManager
//    private lateinit var snackbarContainer: FrameLayout
//    private val _scope = CoroutineScope(Dispatchers.Main)

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        _pluginManager = if (!PluginManager.isInitialized()) {
            PluginManager(this)
        } else {
            Log.e("ee-x", "Don't expect this case")
            PluginManager.instance
        }
    }

    override fun onStart() {
        super.onStart()
        _pluginManager.onActivityStart()
    }

    override fun onResume() {
        super.onResume()
        _pluginManager.onActivityResume()
    }

    override fun onPause() {
        super.onPause()
        _pluginManager.onActivityPause()
    }

    override fun onStop() {
        super.onStop()
        _pluginManager.onActivityStop()
    }

    override fun onDestroy() {
        super.onDestroy()
        _pluginManager.onActivityDestroy()
//        window.decorView.findViewById<ViewGroup>(android.R.id.content).removeView(snackbarContainer)
    }

    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        _pluginManager.onRequestPermissionsResult(requestCode, permissions, grantResults)
    }

    override fun onActivityResult(requestCode: Int, resultCode: Int, data: Intent?) {
        super.onActivityResult(requestCode, resultCode, data)
        _pluginManager.onActivityResult(requestCode, resultCode, data)
    }

//    private fun setupSnackbarFragment() {
//        snackbarContainer = FrameLayout(this).apply {
//            layoutParams = ViewGroup.LayoutParams(
//                ViewGroup.LayoutParams.MATCH_PARENT,
//                ViewGroup.LayoutParams.MATCH_PARENT
//            )
//            elevation = 100f // Đặt trên unitySurfaceView
//            setBackgroundColor(android.graphics.Color.TRANSPARENT)
//        }
//        // Thêm container vào root view
//        window.decorView.findViewById<ViewGroup>(android.R.id.content).addView(snackbarContainer)
//    }
//
//    private fun showSnackbar(message: String, duration: Int = Snackbar.LENGTH_SHORT) {
//        Snackbar
//            .make(snackbarContainer, message, duration)
//            .setAction("Cài đặt") {
//                Log.d("ee-x", "Action clicked")
//            }
//            .show()
//    }
}