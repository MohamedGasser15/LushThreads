import 'package:flutter/material.dart';
import 'package:LushThreads/core/constants/app_routes.dart';
import 'package:LushThreads/core/theme/app_colors.dart';
import 'package:LushThreads/core/theme/app_text_styles.dart';
import 'package:LushThreads/shared/models/cart.dart';


// ─── Checkout Screen ──────────────────────────────────────────────────────────
class CheckoutScreen extends StatefulWidget {
  const CheckoutScreen({super.key});

  @override
  State<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends State<CheckoutScreen> {
  final cart = CartManager.instance;
  int _step = 0; // 0 = address, 1 = payment

  final _nameCtrl    = TextEditingController(text: 'Mohamed Gasser');
  final _addressCtrl = TextEditingController(text: 'Grandplaza Towers, Portsaid');
  final _cityCtrl    = TextEditingController(text: 'Port Said');
  final _cardCtrl    = TextEditingController(text: '4242 4242 4242 4242');
  final _expiryCtrl  = TextEditingController(text: '12/27');
  final _cvvCtrl     = TextEditingController(text: '123');

  @override
  void dispose() {
    _nameCtrl.dispose(); _addressCtrl.dispose(); _cityCtrl.dispose();
    _cardCtrl.dispose(); _expiryCtrl.dispose(); _cvvCtrl.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      appBar: AppBar(
        leading: GestureDetector(
          onTap: () => Navigator.pop(context),
          child: const Icon(Icons.arrow_back_ios_new_rounded, size: 18)),
        title: Text('Checkout', style: AppTextStyles.titleMedium),
        centerTitle: true,
      ),
      body: SafeArea(
        child: Column(
          children: [
            _buildStepper(),
            Expanded(
              child: SingleChildScrollView(
                padding: const EdgeInsets.all(20),
                child: _step == 0 ? _buildAddressForm() : _buildPaymentForm(),
              ),
            ),
            _buildBottomBar(),
          ],
        ),
      ),
    );
  }

  Widget _buildStepper() {
    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 40, vertical: 16),
      child: Row(
        children: [
          _stepCircle(0, 'Shipping'),
          Expanded(child: Container(height: 2,
              color: _step >= 1 ? AppColors.primary : AppColors.divider)),
          _stepCircle(1, 'Payment'),
        ],
      ),
    );
  }

  Widget _stepCircle(int index, String label) {
    final active = _step >= index;
    return Column(children: [
      Container(
        width: 32, height: 32,
        decoration: BoxDecoration(
          color: active ? AppColors.primary : AppColors.surface,
          shape: BoxShape.circle,
          border: Border.all(
            color: active ? AppColors.primary : AppColors.divider),
        ),
        child: Center(
          child: active && _step > index
              ? const Icon(Icons.check, size: 16, color: Colors.white)
              : Text('${index + 1}',
                  style: TextStyle(fontSize: 13, fontWeight: FontWeight.w700,
                      color: active ? Colors.white : AppColors.hint)),
        ),
      ),
      const SizedBox(height: 4),
      Text(label, style: AppTextStyles.labelSmall.copyWith(
          color: active ? AppColors.primary : AppColors.hint)),
    ]);
  }

  Widget _buildAddressForm() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Shipping Address'),
        const SizedBox(height: 16),
        _field('Full Name', _nameCtrl, Icons.person_outline),
        _field('Address', _addressCtrl, Icons.location_on_outlined),
        _field('City', _cityCtrl, Icons.location_city_outlined),
        const SizedBox(height: 12),
        Container(
          padding: const EdgeInsets.all(14),
          decoration: BoxDecoration(
            color: AppColors.primaryLight, borderRadius: BorderRadius.circular(12)),
          child: Row(children: [
            const Icon(Icons.local_shipping_outlined,
                color: AppColors.primary, size: 20),
            const SizedBox(width: 10),
            Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text('Free Shipping', style: AppTextStyles.labelMedium
                    .copyWith(color: AppColors.primary)),
                Text('Estimated delivery: 3–5 business days',
                    style: AppTextStyles.bodySmall),
              ],
            )),
          ]),
        ),
      ],
    );
  }

  Widget _buildPaymentForm() {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        _sectionTitle('Payment Details'),
        const SizedBox(height: 16),
        // Card graphic
        Container(
          height: 160,
          decoration: BoxDecoration(
            gradient: const LinearGradient(
              colors: [AppColors.primary, AppColors.primaryDark],
              begin: Alignment.topLeft, end: Alignment.bottomRight,
            ),
            borderRadius: BorderRadius.circular(20),
          ),
          padding: const EdgeInsets.all(20),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                Text('LushThreads',
                    style: AppTextStyles.labelLarge.copyWith(color: Colors.white)),
                const Icon(Icons.credit_card, color: Colors.white, size: 28),
              ]),
              Text(_cardCtrl.text,
                  style: const TextStyle(color: Colors.white, fontSize: 18,
                      fontWeight: FontWeight.w600, letterSpacing: 2)),
              Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                Text(_nameCtrl.text,
                    style: const TextStyle(color: Colors.white70, fontSize: 13)),
                Text(_expiryCtrl.text,
                    style: const TextStyle(color: Colors.white70, fontSize: 13)),
              ]),
            ],
          ),
        ),
        const SizedBox(height: 20),
        _field('Card Number', _cardCtrl, Icons.credit_card_outlined),
        Row(children: [
          Expanded(child: _field('Expiry (MM/YY)', _expiryCtrl, Icons.calendar_today_outlined)),
          const SizedBox(width: 12),
          Expanded(child: _field('CVV', _cvvCtrl, Icons.lock_outline,
              obscure: true)),
        ]),
        const SizedBox(height: 16),
        _summaryBox(),
      ],
    );
  }

  Widget _summaryBox() {
    return Container(
      padding: const EdgeInsets.all(16),
      decoration: BoxDecoration(
        color: AppColors.surface, borderRadius: BorderRadius.circular(14)),
      child: Column(children: [
        _row('Subtotal', '\$${cart.subtotal.toStringAsFixed(2)}'),
        const SizedBox(height: 6),
        _row('Shipping', cart.shipping == 0 ? 'Free' : '\$${cart.shipping.toStringAsFixed(2)}'),
        const Divider(height: 16, color: AppColors.divider),
        _row('Total', '\$${cart.total.toStringAsFixed(2)}', bold: true),
      ]),
    );
  }

  Widget _row(String a, String b, {bool bold = false}) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.spaceBetween,
      children: [
        Text(a, style: bold ? AppTextStyles.labelLarge : AppTextStyles.bodyMedium),
        Text(b, style: bold
            ? AppTextStyles.priceSmall
            : AppTextStyles.labelMedium),
      ],
    );
  }

  Widget _field(String hint, TextEditingController ctrl, IconData icon,
      {bool obscure = false}) {
    return Padding(
      padding: const EdgeInsets.only(bottom: 12),
      child: TextField(
        controller: ctrl,
        obscureText: obscure,
        decoration: InputDecoration(
          hintText: hint,
          hintStyle: const TextStyle(color: AppColors.hint, fontSize: 14),
          prefixIcon: Icon(icon, size: 18, color: AppColors.hint),
          filled: true, fillColor: AppColors.surface,
          border: OutlineInputBorder(
              borderRadius: BorderRadius.circular(12), borderSide: BorderSide.none),
        ),
      ),
    );
  }

  Widget _sectionTitle(String t) =>
      Text(t, style: AppTextStyles.titleMedium);

  Widget _buildBottomBar() {
    return Container(
      padding: const EdgeInsets.fromLTRB(20, 12, 20, 24),
      decoration: const BoxDecoration(
          color: Colors.white,
          border: Border(top: BorderSide(color: AppColors.divider))),
      child: SizedBox(
        width: double.infinity,
        child: GestureDetector(
          onTap: () {
            if (_step == 0) {
              setState(() => _step = 1);
            } else {
              cart.clear();
              Navigator.pushNamedAndRemoveUntil(
                  context, AppRoutes.orderSuccess, (r) => false);
            }
          },
          child: Container(
            height: 50,
            decoration: BoxDecoration(
              color: AppColors.primary, borderRadius: BorderRadius.circular(14)),
            child: Center(
              child: Text(_step == 0 ? 'Continue to Payment' : 'Place Order',
                  style: AppTextStyles.labelLarge.copyWith(color: Colors.white)),
            ),
          ),
        ),
      ),
    );
  }
}

// ─── Order Success Screen ─────────────────────────────────────────────────────
class OrderSuccessScreen extends StatelessWidget {
  const OrderSuccessScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: AppColors.background,
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(32),
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Container(
                width: 100, height: 100,
                decoration: const BoxDecoration(
                    color: AppColors.primaryLight, shape: BoxShape.circle),
                child: const Icon(Icons.check_circle_rounded,
                    color: AppColors.primary, size: 56),
              ),
              const SizedBox(height: 24),
              Text('Order Placed!', style: AppTextStyles.titleLarge),
              const SizedBox(height: 10),
              Text('Your order has been confirmed.\nYou will receive a confirmation email shortly.',
                  textAlign: TextAlign.center,
                  style: AppTextStyles.bodyMedium),
              const SizedBox(height: 32),
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                    color: AppColors.surface,
                    borderRadius: BorderRadius.circular(16)),
                child: Column(children: [
                  _infoRow(Icons.receipt_long_rounded, 'Order #', 'LT-${DateTime.now().millisecondsSinceEpoch % 100000}'),
                  const SizedBox(height: 10),
                  _infoRow(Icons.local_shipping_outlined, 'Estimated Delivery', '3–5 business days'),
                  const SizedBox(height: 10),
                  _infoRow(Icons.email_outlined, 'Confirmation sent to', 'your email'),
                ]),
              ),
              const SizedBox(height: 32),
              SizedBox(
                width: double.infinity,
                child: GestureDetector(
                  onTap: () => Navigator.pushNamedAndRemoveUntil(
                      context, AppRoutes.home, (r) => false),
                  child: Container(
                    height: 50,
                    decoration: BoxDecoration(
                        color: AppColors.primary,
                        borderRadius: BorderRadius.circular(14)),
                    child: Center(
                      child: Text('Continue Shopping',
                          style: AppTextStyles.labelLarge
                              .copyWith(color: Colors.white)),
                    ),
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }

  static Widget _infoRow(IconData icon, String label, String value) {
    return Row(children: [
      Icon(icon, size: 18, color: AppColors.primary),
      const SizedBox(width: 10),
      Expanded(child: Text(label,
          style: const TextStyle(fontSize: 13, color: AppColors.hint))),
      Text(value, style: const TextStyle(fontSize: 13,
          fontWeight: FontWeight.w600, color: AppColors.textMain)),
    ]);
  }
}
