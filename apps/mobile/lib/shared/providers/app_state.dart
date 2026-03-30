import 'package:shared_preferences/shared_preferences.dart';

class AppState {
  AppState._();
  static final AppState instance = AppState._();

  static const _keyOnboarding = 'hasSeenOnboarding';
  static const _keyGuest      = 'isGuest';
  static const _keyLoggedIn   = 'isLoggedIn';

  // ── In-memory cache ────────────────────────────────────────────────────────
  bool _hasSeenOnboarding = false;
  bool _isLoggedIn        = false;
  bool _isGuest           = false;

  bool get hasSeenOnboarding => _hasSeenOnboarding;
  bool get isLoggedIn        => _isLoggedIn;
  bool get isGuest           => _isGuest;

  // ── Load from SharedPreferences on app start ───────────────────────────────
  Future<void> init() async {
    final prefs = await SharedPreferences.getInstance();
    _hasSeenOnboarding = prefs.getBool(_keyOnboarding) ?? false;
    _isLoggedIn        = prefs.getBool(_keyLoggedIn)   ?? false;
    _isGuest           = prefs.getBool(_keyGuest)      ?? false;
  }

  // ── Actions ────────────────────────────────────────────────────────────────
  Future<void> completeOnboarding() async {
    _hasSeenOnboarding = true;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_keyOnboarding, true);
  }

  Future<void> loginAsGuest() async {
    _isGuest   = true;
    _isLoggedIn = false;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_keyGuest, true);
    await prefs.setBool(_keyLoggedIn, false);
  }

  Future<void> loginAsUser() async {
    _isLoggedIn = true;
    _isGuest    = false;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_keyLoggedIn, true);
    await prefs.setBool(_keyGuest, false);
  }

  Future<void> logout() async {
    _isLoggedIn = false;
    _isGuest    = false;
    final prefs = await SharedPreferences.getInstance();
    await prefs.setBool(_keyLoggedIn, false);
    await prefs.setBool(_keyGuest, false);
  }
}
